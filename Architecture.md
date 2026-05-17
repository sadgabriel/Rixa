# RIXA 아키텍처 문서

## 1. 개요

이 문서는 RIXA의 기술적 구조를 설명한다.

RIXA는 서버-클라이언트 구조의 실시간 멀티플레이 게임으로, Node.js 서버와 Unity 클라이언트가 WebSocket으로 통신한다. 서버가 게임 상태를 단독으로 소유하고 클라이언트는 그 복제본을 유지하는 서버 권위(server-authoritative) 방식을 채택했다. 또한 LLM API를 게임 로직에 직접 통합하여 세계관 생성, 전투 판정, 내러티브 생성을 수행한다.

---

## 2. 전체 구조

### 서버-클라이언트 구조

```
[Unity 클라이언트]                      [Node.js 서버]
                                        
  WsClient                                index.js
  (WebSocket 연결/송수신)    ←—————→    (연결 관리, 메시지 라우팅)
       ↕                                     ↕
  GameClient                             Game
  (메시지 조립/분해)                     (게임 상태 머신)
       ↕                                     ↕
  StateManager                           Judge
  (상태 캐싱, UIState 계산)              (LLM 파이프라인)
       ↕
  UIManager
  (패널 전환)
       ↕
  각 Panel
  (UI 갱신)
```

서버가 게임의 모든 상태를 소유하며, 상태가 변경될 때마다 관련 클라이언트에게 전체 게임 상태를 브로드캐스트한다. 클라이언트는 상태를 직접 변경하지 않고 서버에 요청을 보낸 뒤 응답으로 받은 상태를 반영한다.

### 메시지 프로토콜

모든 메시지는 `{ type, data }` 형태의 JSON으로 전송된다.

**클라이언트 → 서버**

| 타입 | 설명 |
|---|---|
| `client.state` | 클라이언트 상태 요청 |
| `lobby.createGame` | 게임 생성 |
| `lobby.joinGame` | 게임 참가 |
| `lobby.state` | 로비 상태 요청 |
| `game.leave` | 게임 나가기 |
| `game.state` | 게임 상태 요청 |
| `game.submit` | 게임 내 행동 제출 (kind로 세분화) |

`game.submit`은 게임 내 모든 제출 행동을 하나의 타입으로 묶고 `kind` 필드로 세분화한다. 게임 내 행동이 모두 "플레이어가 무언가를 제출한다"는 공통된 의미를 가지므로 단일 진입점으로 처리하기 위해 이 구조를 선택했다.

| kind | 설명 |
|---|---|
| `ready` / `unready` | 준비 상태 변경 |
| `start` | 게임 시작 |
| `context` | 세계관 배경 제출 |
| `factionConcept` | 진영 컨셉 제출 |
| `factionFlaw` | 진영 약점 제출 |
| `contextSetupFinishReady` | 세계관 설정 완료 확인 |
| `attack` | 공격 서술 제출 |
| `defense` | 방어 서술 제출 |
| `competitionFinishReady` | 전투 결과 확인 완료 |
| `cancel` | 제출 취소 |

**서버 → 클라이언트**

| 타입 | 설명 | 대상 |
|---|---|---|
| `welcome` | 접속 시 초기 상태 전송 | 접속한 클라이언트 |
| `client.state` | 클라이언트 상태 | 해당 클라이언트 |
| `lobby.state` | 로비 상태 | 전체 브로드캐스트 |
| `game.state` | 게임 상태 | 게임 내 플레이어 |
| `error` | 오류 | 해당 클라이언트 |

---

## 3. 서버

### 게임 상태 머신

게임의 전체 흐름은 페이즈(phase) 기반 상태 머신으로 관리된다. 모든 플레이어 액션은 현재 페이즈에 대해 검증되며, 페이즈 전환은 서버에서만 이루어진다.

```
LOBBY
  ↓ (모든 플레이어 준비 완료 후 방장이 시작)
CONTEXT_INPUT
  ↓ (리더가 세계관 배경 제출)
FACTION_CONCEPT_INPUT
  ↓ (모든 플레이어 진영 컨셉 제출)
FACTION_FLAW_INPUT
  ↓ (모든 플레이어 약점 제출 완료)
CONTEXT_SETUP ————————→ LLM 호출 (Context Setup)
  ↓
CONTEXT_SETUP_FINISH
  ↓ (모든 플레이어 확인)
ATTACK
  ↓ (모든 플레이어 공격 제출)
DEFENSE
  ↓ (모든 플레이어 방어 제출 완료)
COMPETITION_ANALYZE ——→ LLM 호출 (Competition Analyze)
  ↓
COMPETITION_NARRATE ———→ LLM 호출 (Competition Narrate)
  ↓
COMPETITION_FINISH
  ↓ (모든 플레이어 확인)
  ├─ 라운드 남음 → round++ → ATTACK
  └─ 마지막 라운드 → END
```

LLM 호출이 필요한 페이즈(CONTEXT_SETUP, COMPETITION_ANALYZE, COMPETITION_NARRATE)에서는 마지막 플레이어의 제출이 LLM 호출을 트리거한다. 호출이 완료되면 서버가 페이즈를 전환하고 새 게임 상태를 브로드캐스트한다. 클라이언트는 이 기간 동안 대기 화면을 표시한다.

LLM 호출이 완료된 뒤 게임이 END 페이즈에 도달하면 5초 대기 후 자동으로 reset되어 같은 멤버로 LOBBY 상태로 돌아간다.

### 매치 편성 알고리즘

매 라운드 공격자-방어자 쌍은 라운드 오프셋 방식으로 결정된다.

게임 시작 시 플레이어 순서를 무작위로 섞어 `playerCycle`을 만든다. 이후 각 라운드마다 오프셋 값을 하나씩 적용하여 매치를 생성한다. 플레이어 수가 N명일 때 오프셋은 1부터 N-1까지의 값을 순환한다.

예시 (4인, 오프셋=1):
```
playerCycle: [A, B, C, D]
A → B, B → C, C → D, D → A
```

예시 (4인, 오프셋=2):
```
playerCycle: [A, B, C, D]
A → C, B → D, C → A, D → B
```

오프셋이 한 사이클(1~N-1)을 완료하면 모든 플레이어가 서로를 한 번씩 공격한 것이 된다. 게임은 이 사이클을 totalRounds가 될 때까지 반복한다.

### 플레이어 수 연동 밸런싱

플레이어 수에 따라 총 라운드 수와 초기 자원량이 자동 조정된다. 모든 플레이어가 서로를 최대 2번씩 만날 수 있도록 라운드 수를 설계했다.

| 플레이어 수 | 총 라운드 | 초기 자원량 |
|:---:|:---:|:---:|
| 2 | 2 | 1 |
| 3 | 4 | 1 |
| 4 | 6 | 2 |
| 5 | 4 | 1 |
| 6 | 5 | 2 |

자원량은 총 라운드가 4 이하이면 1, 5 이상이면 2로 설정된다. 라운드가 짧을수록 자원이 적어 자원이 더 빠르게 고갈된다.

---

## 4. LLM 파이프라인

### 3단계 구조

RIXA에서 LLM은 세 가지 목적으로 사용된다. 승패 판정 자체는 LLM이 아닌 난수로 결정하며, LLM은 그 확률을 조정하는 데이터를 생성하는 역할을 한다.

**Phase 1: Context Setup**

플레이어가 입력한 세계관 배경, 진영 컨셉, 약점을 바탕으로 LLM이 게임에 사용할 정제된 데이터를 생성한다.

- 입력: 세계관 배경 설명, 각 진영의 컨셉/약점 (플레이어 원문)
- 출력: 정제된 세계관 요약, 진영별 설명 및 자원 3종

**Phase 2: Competition Analyze**

플레이어의 공격/방어 서술을 분석하여 승률 계산에 사용할 태그를 추출한다.

- 입력: 공격 서술, 방어 서술
- 출력: 각 서술의 긍정/부정 태그 목록, 공격이 노리는 자원, 방어가 보호하는 자원

**Phase 3: Competition Narrate**

판정 결과를 바탕으로 세계관 내 내러티브를 생성한다.

- 입력: 매치 결과 (승자, 잃은 자원)
- 출력: 각 매치의 전투 묘사, 이벤트 로그 업데이트

모든 LLM 호출은 JSON Schema로 출력 형식을 강제하여 서버가 응답을 별도 파싱 없이 직접 사용할 수 있도록 했다.

### 태그 기반 승률 계산

LLM이 추출한 태그를 기반으로 승률을 조정한 뒤 난수로 승패를 결정한다.

**긍정 태그** (각 +0.05):
`coherence`, `evidence_and_reasoning`, `situational_leverage`, `anticipation_and_counterplay`, `creativity`, `deception`, `pressure`, `escalation`, `high_variance`

**부정 태그** (각 -0.05):
`nonresponsive`, `unsupported_or_logical_gap`, `self_contradiction_or_overreach`

```
승률 = 기본 승률(0.50)
      + (공격자 태그 합산)
      - (방어자 태그 합산)

승률 범위: 최소 0.20 ~ 최대 0.80
```

공격자가 승리하면 방어자는 자원 중 하나를 잃는다. 잃는 자원은 LLM이 분석한 `targeted_resources`와 `protected_resources`를 가중치로 활용한 가중 랜덤으로 결정된다.

### 오류 처리

LLM 호출은 네트워크 불안정이나 응답 지연에 대비해 타임아웃과 지수 백오프 재시도 로직을 포함한다.

```
1회 시도 → 타임아웃 시 실패
실패 시 1초 대기 후 재시도
재시도 실패 시 2초 대기 후 재시도
최대 3회 시도 후 에러 반환
```

응답이 오더라도 JSON Schema를 만족하지 않는 경우(`InvalidJudgeResponseError`)는 재시도 없이 즉시 에러를 반환한다. 이는 잘못된 형식의 응답이 반복될 가능성이 낮고, 재시도해도 같은 결과가 나올 가능성이 높기 때문이다.

---

## 5. Unity 클라이언트

### 상태 동기화 구조

서버에서 `game.state`가 브로드캐스트되면 다음 흐름으로 UI까지 반영된다.

```
서버 브로드캐스트 (game.state)
  → WsClient: WebSocket 수신, JSON 파싱, OnMessage 이벤트 발행
  → GameClient: 메시지 타입별 분기, 역직렬화, OnGameStateUpdated 이벤트 발행
  → StateManager: 게임 상태 캐싱, 서버 페이즈를 UIState로 변환, OnUIStateUpdated 이벤트 발행
  → UIManager: UIState에 따라 패널 전환
  → 각 Panel: OnEnable/이벤트 구독을 통해 UI 갱신
```

StateManager는 서버의 게임 페이즈(문자열)를 클라이언트의 UIState(열거형)로 변환하는 역할을 한다. 서버의 `competition_analyze`와 `competition_narrate` 페이즈는 모두 클라이언트에서 `GAME_COMPETITION_CALCULATE`로 매핑된다. LLM 처리 중에는 클라이언트 입장에서 두 페이즈를 구분할 필요가 없기 때문이다.

StateManager는 게임 상태의 캐싱 외에도 자주 필요한 조회(`MyPlayer`, `MyFaction`, `AnotherFaction`, `MyAttackMatch` 등)를 편의를 위해 프로퍼티로 제공한다.

### UI 시스템

UI 상태를 UIContext와 UIState 두 단계로 나누어 관리한다.

**UIContext**는 장기적인 맥락을 나타내며 APP과 GAME 두 가지만 존재한다. 게임 방에 입장하기 전은 APP, 입장 후는 GAME이다.

**UIState**는 게임 내 세부 상태를 나타내며 서버의 페이즈와 대응된다.

이 구분에 따라 패널을 두 종류로 나눴다.

- **PersistentPanel**: UIContext 단위로 표시/숨김. 페이즈가 바뀌어도 유지되어야 하는 UI (예: 플레이어 정보창, 입력 영역)
- **NonPersistentPanel**: UIState 단위로 교체. 페이즈마다 달라지는 UI (예: 각 입력 단계 안내, 대기 화면, 전투 결과)

이렇게 분리한 이유는 재활용성 때문이다. 서버 페이즈가 바뀌어도 유지되어야 하는 UI와 매번 교체되어야 하는 UI를 하나의 구조로 관리하면 중복 구현이 늘어나므로, 두 계층으로 나누어 각각 독립적으로 관리하도록 했다.

### 다이얼로그 시스템

패널 시스템과는 별개로 다이얼로그 시스템을 구현했다. 다이얼로그는 게임의 흐름과 관계없이 어느 상태에서든 띄울 수 있어야 하며, 여러 개가 중첩될 수 있어야 한다.

`DialogManager`는 스택 구조로 다이얼로그를 관리한다. 새 다이얼로그가 열리면 스택에 쌓이고, ESC 키나 취소 버튼을 누르면 최상단 다이얼로그가 닫힌다. 스택이 비어있는 상태에서 ESC를 누르면 게임 종료 확인 다이얼로그가 열린다.

Spider 프로젝트에서는 패널 기반 UI 시스템만 존재했으나, RIXA에서는 확인창, 진영 정보창, 입력창 등 다양한 다이얼로그가 필요하여 별도 시스템으로 분리했다.