# Danmaku 코드베이스 안내서

## 목차

1. [프로젝트 개요와 폴더 구조](#1-프로젝트-개요와-폴더-구조)
2. [아키텍처와 주요 시스템의 역할 및 데이터 흐름](#2-아키텍처와-주요-시스템의-역할-및-데이터-흐름)
3. [데이터 에셋과 ScriptableObject 계층](#3-데이터-에셋과-scriptableobject-계층)
4. [클래스별 책임과 주요 메서드](#4-클래스별-책임과-주요-메서드)

## 1. 프로젝트 개요와 폴더 구조

Danmaku는 Unity 6에서 동작하는 2D 탄막 게임이다. 게임 진행 데이터는 `ScriptableObject` 에셋으로 정의하고, 런타임 시스템은 `MonoBehaviour` 관리자와 컴포넌트로 실행한다. 탄환은 `GameObject` 대신 일반 C# 객체로 유지하며, 고정 프레임에서 이동·충돌을 처리하고 `BulletRenderer`가 화면에 그린다.

주요 폴더는 다음과 같다.

| 경로 | 내용 |
|---|---|
| `Assets/01.Scenes/` | Unity 씬 |
| `Assets/02.Scripts/` | 런타임, 편집기, 테스트용 C# 코드 |
| `Assets/Datas/` | Stage, Spell, Burst, Bullet, Sequence, Player 등의 게임 데이터 에셋 |
| `Assets/03.Textures/` | 스프라이트와 텍스처 |
| `Assets/04.Materials/` | 머티리얼 |
| `Assets/05.Prefabs/` | 게임 오브젝트 프리팹 |

`Assets/02.Scripts/` 내부는 기능별로 나뉜다.

| 경로 | 담당 영역 |
|---|---|
| `Core/` | 싱글턴 기반, 고정 프레임 대기, 설정·저장, 점수 API |
| `Stage/`, `Level/` | Stage·Phase 데이터와 Level·Stage 실행 |
| `Enemy/` | 적 패턴 데이터, 적 런타임, 보스·애니메이션 |
| `Spell/` | SpellCard, Burst, Bullet 및 이동 시퀀스 |
| `Managers/` | 게임·적·탄환·상태·UI·오디오 관리자와 탄환 렌더러 |
| `Player/`, `Input/` | 플레이어 데이터·컨트롤·애니메이션, 입력 이벤트 |
| `UI/` | 버튼, 씬 이동, 화면 캡처 기반 배경 흐림 |
| `Editor/` | 커스텀 인스펙터, 미리보기, 씬 Gizmo |
| `Test/` | 점수 조회와 Spell 발사 테스트 컴포넌트 |

`Editor/`의 코드는 `UnityEditor`에 의존하며 편집기 전용이다. 런타임 코드는 별도의 `Editor/` 디렉터리에 두어 플레이어 빌드에서 편집기 전용 API를 참조하지 않는다.

## 2. 아키텍처와 주요 시스템의 역할 및 데이터 흐름

### 시간과 관리자

게임 진행은 주로 `FixedUpdate`와 `Time.fixedDeltaTime`을 기준으로 한다. `WaitForFixedFrame`은 지정 프레임 수만큼 기다리는 IEnumerator이며, `GameManager.WaitForFixedUpdate`는 고정 프레임 대기에 재사용된다. 시간 변환은 `GameManager.FrameToSecond`와 `GameManager.SecondToFrame`이 초당 60프레임을 기준으로 한다.

`SingletonBehaviour<T>`를 상속한 관리자는 씬에서 활성 인스턴스를 찾고, 필요한 경우 `Resources/ManagerObjects/`에서 해당 관리자 프리팹을 생성한다. `GameManager`와 `AudioManager`는 씬 전환 뒤에도 유지된다. `GameManager.Pause`와 `Resume`은 중첩 일시정지를 카운트하고, 단일 씬 로드 시 일시정지 상태와 `Time.timeScale`을 초기화한다.

### Stage와 Level

`LevelData`가 `StageData` 목록을 보유한다. `LevelRunner.Initialize`는 목록 열거자를 만들고, `StartLevel`이 다음 Stage를 `StageRunner`에 전달해 실행한다. 현재 `LevelRunner`는 Stage 종료 이벤트를 구독하지 않으므로, Stage가 끝난 뒤 다음 Stage를 자동으로 시작하는 연결은 코드에 없다. Stage가 모두 소진되면 `SummitScore` 씬을 추가로 로드한다.

`StageRunner.StartRun`은 Phase를 순서대로 진행한다. Phase 시작 프레임만큼 대기한 뒤 `EnemyChain`의 적 패턴들을 차례로 생성하고, 각 생성 사이에 Chain의 대기 프레임만큼 기다린다. `PhaseData.Boss.Type`이 `Common`이 아니면 보스를 생성하고 `EnemyBoss.BossAlive`가 끝날 때까지 기다린다. 모든 Phase 뒤에는 300 고정 프레임을 기다린 다음 게임을 멈추고 결과 점수와 결과 UI를 표시한다.

`StageRunner.Initialize`는 실행할 `StageData`를 지정한다. InGame 씬의 `StageRunner`는 편집기 `StageRunEditor`의 시작 버튼으로 실행할 수 있다.

### Enemy와 SpellCard

`EnemyManager.SpawnEnemy`는 `EnemyPattern.Type`에 따라 일반 적을 풀에서 가져오거나 보스 프리팹을 생성한다. 이어서 애니메이터 컨트롤러를 지정하고 `Enemy.Initialize`와 `Enemy.StartPattern`을 호출한다. 일반 적은 오브젝트 풀로 반환하고, 보스는 사망 퇴장 트윈 후 파괴한다.

`Enemy.StartPattern`은 `EnemyPattern.Spells`를 순회한다. Spell마다 HP를 `PresentHP`로 설정하고 `OnSpellStart`를 호출한 뒤, 적의 첫 버스트 대기 프레임이 있으면 대기한다. `BulletManager.BurstChain`을 실행하고 `TimeLimit`이 지나거나 HP가 0 이하가 될 때까지 고정 프레임 단위로 확인한다. 버스트 체인이 정상 완료되고 Spell이 계속 유효하면 체인을 다시 실행한다. HP가 0 이하일 때는 아이템 드롭 코루틴을 시작한 뒤 다음 Spell로 넘어간다. 모든 Spell이 끝나면 `OnDie`가 점수를 더하고 적을 반환한다. 보스는 Spell 시작 시 보스 HP UI를 갱신하고, 사망 시 화면 위쪽으로 퇴장한다.

### Burst와 Bullet

`SpellCard.Bursts`는 여러 `BurstData`의 순서 있는 목록이다. `BulletManager.BurstChain`은 각 Burst의 `BulletPattern`을 순서대로 발사한다. Burst의 `WaitForFrame`이 양수이면 Presenter를 `PresentPosition`까지 선형 이동시키고, 패턴 발사 후 해당 프레임만큼 대기한다. 각 `BulletPattern.WaitForFrame`은 같은 Burst 안에서 다음 탄환을 발사하기 전의 간격이다.

`BulletManager.SpawnBullet`은 `Bullet` 객체를 풀에서 가져와 `BulletPattern`과 Presenter 위치로 초기화하고, 시퀀스를 시작한 다음 활성 목록에 추가한다. 매 고정 프레임마다 살아 있는 탄환을 갱신하고 데드존 밖 탄환을 풀에 반환한 뒤 `BulletCollision.Rebuild`로 충돌 격자를 구성한다. `Bullet`의 위치·속도·토크·피해량·탄환 데이터는 `BulletPattern`에서 초기화된다.

### Bullet sequence와 충돌

`BulletSequenceBase`는 ScriptableObject 기반 이동 설정이다. `GetSequenceRunner`가 런타임 실행자를 제공하고, 실행자는 탄환 생성 시 `Start`를 한 번, 이후 매 프레임 `Next`를 호출한다. `DefaultSequence`는 직선 이동과 토크 회전을, `AccelateSequence`는 커브 기반 속력 변경과 이후 가속을 수행한다. `TargettingSequence`는 생성 시 플레이어를 향하고, `NearestEnemyTargettingSequence`는 가장 가까운 적을 찾으며, `RandomDirectionSequence`는 초기 속력 범위 내 무작위 방향을 정한다. `DownwardSequence`는 아래 방향으로 속도를 맞춘다. `SequenceBridge`는 설정한 프레임 간격으로 여러 시퀀스를 연결한다.

`BulletTag`는 발사 주체 또는 아이템 종류를 비트 플래그로 표현한다. `BulletCollision`은 태그 비트별 균일 격자를 구성하며, 원형 탄환은 원 대 원, Laser는 선분 대 원으로 충돌을 판정한다. `PlayerController`는 `Enemy` 태그 탄환을 검사하고, `Enemy`는 `Player` 태그 탄환을 검사한다. `BulletManager.BulletClearing`은 `Enemy` 탄환을 범위 확장 방식으로 제거한다.

### 렌더링

`BulletRenderer`는 `BulletManager.ActiveBullets`를 읽기 전용으로 참조하고, 활성 탄환을 스프라이트별로 그룹화해 `Graphics.DrawMeshInstanced`로 그린다. 스프라이트 정점과 UV를 이용해 메시를 생성하고, 배치별 텍스처와 인스턴스별 색상·변환 행렬을 전달한다. 기본적으로 탄환 속도 방향에 맞춰 스프라이트를 회전한다. 렌더러는 탄환 데이터를 변경하지 않는다.

### 상태와 UI

`StatusManager`는 HP·Power·점수·최고 점수, 피격·폭탄·사망 이벤트를 관리한다. HP와 Power는 내부에서 100 단위 정수로 저장하며 `HP`와 `Power`는 100으로 나눈 값을 제공한다. 피격 후 18프레임에 HP가 100 감소하고, HP가 0 미만이면 최고 점수를 저장하고 게임 오버 처리한다. 폭탄과 반폭탄은 각각 Power 100, 50을 소모한다.

`UIManager.Awake`는 `StatusManager` 이벤트를 구독해 HP·Power·점수 UI를 갱신한다. 보스 이름과 체력, 결과 패널과 결과 가산 점수를 표시하는 메서드도 제공한다. `PlayerAnimation`은 입력과 상태 이벤트에 따라 이동 방향, 피격·복귀·폭탄 애니메이션을 처리한다.

### 입력과 플레이어

`InGameInputHandler`는 Unity Input System 콜백을 이동·느린 이동·폭탄 이벤트로 변환한다. `PlayerController`는 플레이어 데이터와 입력 이벤트를 사용해 이동, 탄환 발사, 피격, 폭탄 처리를 수행한다. 이동과 공격은 고정 프레임에서 처리하며 플레이어는 `Deadzone`의 `Min`·`Max` 범위 안에서 움직인다. 공격은 현재 Power 단계의 `BurstData`를 5프레임 간격으로 발사한다.

아이템 흡입 및 획득 로직은 `PlayerController.InhaleItem`에 구현되어 있지만, 코드에서 이 메서드를 호출하는 곳은 없다. `InGameSwipe`는 드래그 거리가 설정값 이상일 때 `InGameInputHandler.Swipe`를 호출하지만, 입력 핸들러의 `OnSwipe` 이벤트를 구독하는 게임 로직은 이 코드 범위에서 확인되지 않는다.

## 3. 데이터 에셋과 ScriptableObject 계층

실제 코드의 데이터 연결은 다음과 같다.

```text
LevelData
└── StageData[]
    └── PhaseData[]
        ├── EnemyChain[]
        │   └── EnemyPattern[]
        │       ├── SpellCard[]
        │       │   └── BurstData[]
        │       │       └── BulletPattern[]
        │       └── Body: BulletData
        └── Boss: EnemyPattern
```

요청에 제시된 계층의 `EnemyData` 타입은 `Assets/02.Scripts/` 코드에 존재하지 않는다. 적 데이터는 별도 ScriptableObject가 아니라 `EnemyChain.EnemyPatterns`와 `PhaseData.Boss`에 저장된 `EnemyPattern` 구조체에 직접 정의되어 있다.

### 진행 및 적 데이터

| 타입·필드 | 의미 |
|---|---|
| `LevelData.stages` / `Stages` | 레벨에 포함되는 `StageData` 목록 |
| `LevelData.ScoreMultiplyer` | 점수 배율 필드. 현재 보이는 런타임 흐름에서는 사용되지 않는다. |
| `StageData.StageID` | Stage 번호 |
| `StageData.StageTitle` | Stage 제목 |
| `StageData.BGM` | Stage에 연결된 배경 음악 |
| `StageData.Phases` | 순서대로 실행할 Phase 목록 |
| `PhaseData.WaitForFirstEmenyFrame` | Phase 시작 전 대기 프레임 |
| `PhaseData.EnemyChains` | Phase 내 적 그룹 목록 |
| `PhaseData.Boss` | Phase의 보스 패턴. `Type`이 `Common`이면 보스 없이 진행한다. |
| `EnemyChain.EnemyPatterns` | Chain에서 차례대로 생성할 적 패턴 |
| `EnemyChain.WaitForNextEmenyFrame` | Chain 안의 적 생성 사이 대기 프레임 |
| `EnemyPattern.Type` | 적 분류: `Common`, `MiddleBoss`, `LastBoss` |
| `EnemyPattern.EnemyName` | 적 또는 보스 이름 |
| `EnemyPattern.Description` | 적 설명 |
| `EnemyPattern.EnemyColor` | 적 렌더러 색상 |
| `EnemyPattern.Score` | 적 사망 시 추가할 점수 |
| `EnemyPattern.Body` | 적 몸체용 `BulletData`. 스프라이트, 충돌 반경, 태그를 참조한다. |
| `EnemyPattern.AniCon` | 적 Animator에 지정할 `RuntimeAnimatorController` |
| `EnemyPattern.SpawnPosition` | 적 생성 위치 |
| `EnemyPattern.DropPowerCount` | 처치 시 생성할 Power 아이템 수 |
| `EnemyPattern.DropScoreCount` | 처치 시 생성할 Score 아이템 수 |
| `EnemyPattern.WaitForFirstBurstFrame` | Spell 실행 전에 적이 기다릴 프레임 |
| `EnemyPattern.Spells` | 내부 `spells` 배열을 읽기 전용 목록으로 노출하는 Spell 목록 |

### Spell과 발사 데이터

| 타입·필드 | 의미 |
|---|---|
| `SpellCard.SpellName` | Spell 이름 |
| `SpellCard.SpellDescription` | Spell 설명 |
| `SpellCard.PresentHP` | Spell 시작 시 설정되는 적 HP |
| `SpellCard.TimeLimit` | Spell 실행 제한 프레임 |
| `SpellCard.Bursts` | Spell에서 반복 실행할 `BurstData` 열거 목록 |
| `BurstData.PresentPosition` | Burst에서 Presenter를 이동시킬 목표 위치 |
| `BurstData.WaitForFrame` | Burst 실행 뒤 대기하고 이동 트윈에 사용할 프레임 수 |
| `BurstData.BulletPatterns` | Burst에 포함된 탄환 발사 패턴 목록 |
| `BulletPattern.BulletData` | 탄환의 종류, 충돌 태그, 스프라이트, 크기 데이터 |
| `BulletPattern.Damage` | 탄환이 입히는 피해량 |
| `BulletPattern.InitialBullet` | 생성 시 색상, 위치, 속도, 토크 데이터 |
| `BulletPattern.BulletSequence` | 탄환 이동을 제어하는 시퀀스 에셋 |
| `BulletPattern.WaitForFrame` | 해당 탄환을 발사한 뒤 다음 패턴 전까지 기다릴 프레임 |
| `InitialBullet.BulletColor` | 탄환 색상 |
| `InitialBullet.InitialPosition` | Presenter 기준 초기 위치 오프셋 |
| `InitialBullet.InitialVelocity` | 초기 속도 벡터 |
| `InitialBullet.InitialTorqueDegree` | 회전에 적용할 초기 토크 각도(도) |

### 탄환과 플레이어 데이터

| 타입·필드 | 의미 |
|---|---|
| `BulletData.Type` | `Circle` 또는 `Laser` |
| `BulletData.Tag` | 탄환 충돌에 사용하는 `BulletTag` 비트 플래그 |
| `BulletData.BulletImage` | 탄환 스프라이트 |
| `BulletData.Distance` | Circle 반지름 또는 Laser 길이 |
| `PlayerData.Speed` | 일반 이동 속도 |
| `PlayerData.SlowSpeed` | 느린 이동 속도 |
| `PlayerData.InitialHP` | 시작 HP(100 단위 정수) |
| `PlayerData.InitialPower` | 시작 Power(100 단위 정수) |
| `PlayerData.PlayerAttacks` | Power 단계별 플레이어 공격 Burst 목록 |
| `PlayerData.PlayerSpell` | 플레이어 Spell 참조 |
| `PlayerData.AniCon` | 플레이어 Animator 컨트롤러 |

Sequence 데이터의 추가 설정은 `AccelateSequence.TargetVelocity`, `Curve`, `Duration`, `Acceleration`; `NearestEnemyTargettingSequence.Side`, `AreaMin`, `AreaMax`; `SequenceBridge.sequences`, `frameBetweenSequence`로 구성된다. 나머지 시퀀스 에셋은 설정 필드 없이 해당 이동 동작을 제공한다.

## 4. 클래스별 책임과 주요 메서드

### `Core/`

| 타입 | 책임 및 주요 메서드 |
|---|---|
| `SingletonBehaviour<T>` | 활성 싱글턴을 찾거나 `Resources/ManagerObjects/`에서 생성한다. `Instance`, `ExistingInstance` 제공. |
| `WaitForFixedFrame` | 지정된 고정 프레임 수만큼 코루틴 대기. `MoveNext`, `Reset`. |
| `Settings` | 오디오 음량과 입력 모드·위치·버튼·HUD 위치 설정 구조체. |
| `DataSaveLoad` | PlayerPrefs 설정·최고 점수와 서버 점수를 저장·조회. `SaveSettings`, `LoadSettings`, `PostScores`, `GetScores`. |
| `ScoreEntry` | 서버 점수 항목 데이터이며 점수·시간 기준 정렬을 위한 `CompareTo` 제공. |
| `DatabaseAPI` | `DataSaveLoad` 점수 코루틴을 시작하는 컴포넌트. `PostScore`, `GetScores`. |
| `SettingPanel` | 설정 UI 값을 `AudioManager`와 `GameManager`에 반영. `SetBGM`, `SetSFX`, `Apply`, `Return`. |
| `ScoreSummit` | 점수판 조회·표시와 점수 제출 및 화면 복귀 처리. `InputName`, `SummitName`, `ReturnButton`. |

### `Level/`, `Stage/`, `Enemy/`

| 타입 | 책임 및 주요 메서드 |
|---|---|
| `LevelData` | Stage 목록과 점수 배율 데이터 |
| `LevelRunner` | 레벨의 Stage 열거·실행. `Initialize`, `StartLevel`. |
| `LevelSelector` | 레벨·캐릭터 선택 및 게임 시작. `SelectLevel`, `SelectCharacter`, `Confirm`, `ReturnToTitle`. |
| `StageData` | Stage 식별자·제목·BGM·Phase 데이터 |
| `PhaseData` | Chain, 첫 적 대기 시간, 보스 패턴 데이터 |
| `EnemyChain` | 적 패턴 목록과 적 생성 간격 데이터 구조체 |
| `StageRunner` | Phase·적·보스 순차 실행과 Stage 결과 처리. `Initialize`, `StartRun`; `OnStageStart`, `OnStageEnd` 이벤트 제공. |
| `EnemyType` | `Common`, `MiddleBoss`, `LastBoss` 구분 |
| `EnemyPattern` | 적 본체·외형·생성·점수·드롭·Spell 설정 구조체 |
| `Enemy` | 적의 Spell 실행, 피격, 아이템 드롭, 점수와 반환 처리. `Initialize`, `StartPattern`; `OnSpellStart`, `OnDie`는 하위 클래스 확장 지점. |
| `EnemyBoss` | 보스 UI, Spell별 HP 갱신, 사망 퇴장 처리. `Initialize`, `BossAlive`. |
| `EnemyAnimation` | 적의 수평 이동을 Animator의 `Right` 매개변수로 전달 |

### `Spell/`

| 타입 | 책임 및 주요 메서드 |
|---|---|
| `SpellCard` | 이름·설명·HP·제한 시간·Burst 목록을 보유하는 ScriptableObject |
| `BurstData` | 목표 위치·대기 시간·탄환 패턴 목록을 보유하는 ScriptableObject |
| `BulletPattern` | 탄환 종류·피해·초기값·이동 시퀀스·발사 간격 구조체 |
| `InitialBullet` | 초기 색상·위치·속도·토크 구조체 |
| `BulletData` | 탄환 스프라이트·유형·태그·크기 ScriptableObject |
| `BulletType` | `Circle`, `Laser` 유형 |
| `BulletTag` | `Enemy`, `Player`, `Power`, `Score`, `Item` 비트 플래그 |
| `Bullet` | 탄환 한 발의 데이터와 이동 상태. `Initialize`, `Clear`, `Start`, `Update`. |
| `BulletCollision` | 태그별 공간 격자를 만들고 탄환 충돌 검사. `Rebuild`, `Check`, `CheckAll`. |
| `BulletSequenceBase` | ScriptableObject 기반 시퀀스 추상 타입. `GetSequenceRunner`; 내부 실행자 인터페이스는 `Start`, `Next` 제공. |
| `DefaultSequence` | 고정 속도 직선 이동과 토크 회전. `GetSequenceRunner`. |
| `AccelateSequence` | 곡선으로 목표 속도에 접근한 뒤 가속·토크 회전. `GetSequenceRunner`. |
| `TargettingSequence` | 생성 시 플레이어를 조준하고 이후 직선 이동. `GetSequenceRunner`. |
| `NearestEnemyTargettingSequence` | 생성 시 지정 영역 내 최근접 적을 조준하며 좌우 우선 방향 설정 가능. `GetSequenceRunner`. |
| `RandomDirectionSequence` | 초기 속력 이하의 무작위 방향 설정 후 이동. `GetSequenceRunner`. |
| `DownwardSequence` | 기존 속력을 유지하며 아래로 이동. `GetSequenceRunner`. |
| `SequenceBridge` | 프레임 경과에 따라 시퀀스를 전환. `GetSequenceRunner`. |

### `Managers/`

| 타입 | 책임 및 주요 메서드 |
|---|---|
| `GameManager` | 일시정지·씬 로드·설정과 InGame 오브젝트 생성. `Pause`, `Resume`, `LoadScene`, `LoadSceneAdditive`, `LoadInGameScene`, `ApplySettings`, `FrameToSecond`, `SecondToFrame`. |
| `AudioManager` | BGM·SFX 음량 및 재생 제어. `SetBGMVolume`, `SetSFXVolume`, `PlayBGM`, `StopBGM`, `PlayOnShotSFX`. |
| `EnemyManager` | 일반 적 풀과 보스 생성·반환. `SpawnEnemy`, `ReleaseEnemy`; `OnRaiseBoss` 이벤트 제공. |
| `BulletManager` | 탄환 풀, Burst 실행, 아이템 생성, 탄환 제거·갱신. `BulletClearing`, `SpawnBullet`, `SpawnPowerItem`, `SpawnScoreItem`, `BulletPatternChain`, `BurstChain`. |
| `BulletRenderer` | 활성 탄환 목록을 스프라이트별 GPU 인스턴싱으로 출력. `SetSource`. |
| `StatusManager` | HP·Power·점수·피격·폭탄·게임 오버 상태 관리. `Initialize`, `AddHP`, `AddPower`, `AddScore`, `CalculateResultScore`, `Hit`, `Retry`, `TryUsePower`, `TryUseSemiPower`; UI 및 상태 이벤트 제공. |
| `UIManager` | HP·Power·점수·보스·결과 UI 갱신. `SetHPSlider`, `SetPowerSlider`, `SetScoreText`, `SetHighScoreText`, `BossStatusOn`, `BossStatusOff`, `SetBossName`, `SetBossHP`, `ResultBoardOn`, 결과 점수 설정 메서드, `OpenRetryPanel`. |

### `Player/`, `Input/`, `UI/`

| 타입 | 책임 및 주요 메서드 |
|---|---|
| `PlayerData` | 플레이어 속도·시작 상태·공격·Spell·애니메이터 데이터 |
| `PlayerController` | 이동·공격·피격·폭탄·아이템 흡입 동작. `Initialize`; `OnInvisibility` 이벤트 제공. |
| `PlayerAnimation` | 이동·피격·복귀·폭탄 상태에 맞춘 Animator, 스케일, 투명도 제어 |
| `InGameInputHandler` | Input System 입력을 게임 이벤트로 전달. `InvokeOnMove`, `InvokeOnUseBomb`, `InvokeOnUseSemiBomb`, `ChangeSlowMoveState`, `Swipe`; 이동·폭탄·스와이프 이벤트 제공. |
| `InGameSwipe` | 포인터 드래그를 감지해 일정 거리 이상일 때 스와이프 전달. `OnPointerDown`, `OnPointerUp`. |
| `HoldButton` | 포인터 누름·해제를 `UnityEvent<bool>`로 전달. `OnPointerDown`, `OnPointerUp`. |
| `TapButton` | 클릭을 `OnTap` 이벤트로 전달. `OnPointerClick`. |
| `LevelContinueFinder` | 버튼 클릭을 찾아 `LevelRunner.StartLevel`에 연결 |
| `LoadSceneProxy` | 버튼 클릭 시 `GameManager.LoadScene` 호출 |
| `LoadSceneAdditiveProxy` | 버튼 클릭 시 `GameManager.LoadSceneAdditive` 호출 |
| `BackgroundBlur` | 활성화 시 화면을 캡처해 셰이더 전역 텍스처로 전달 |

### `Editor/`, `Test/`

| 타입 | 책임 및 주요 메서드 |
|---|---|
| `BulletDataEditor` | `BulletData` Inspector에서 데이터 필드와 스프라이트·판정 도형 미리보기 표시. `OnInspectorGUI`. |
| `BurstDataEditor` | 실제 Bullet·Sequence 코드로 Burst 탄환을 시뮬레이션하고 궤적 미리보기 표시. `OnInspectorGUI`. |
| `DeadzoneGizmos` | 플레이어와 탄환 데드존 사각형을 Scene 뷰에 표시 |
| `EnemyEditor` | 플레이 모드에서 현재 Spell 또는 모든 적 처치 테스트 UI 제공. `OnInspectorGUI`. |
| `StageRunEditor` | 플레이 모드에서 StageRunner 시작 버튼 제공. `OnInspectorGUI`. |
| `StatusManagerEditor` | 플레이 모드에서 HP·Power·피격·폭탄 테스트 UI 제공. `OnInspectorGUI`. |
| `UIManagerEditor` | Inspector에서 상태·보스 UI를 직접 조작하는 테스트 UI 제공. `OnInspectorGUI`. |
| `BulletTest` | 연결된 SpellCard의 Burst들을 현재 Transform에서 발사. `SpellOneShot`. |
| `DBTest` | 점수 서버에서 점수를 조회해 텍스트 UI에 표시. `LoadScore`. |

주요 타입에 포함된 비공개 구현 타입으로는 충돌 격자의 `BulletCollision.Grid`, 스프라이트별 렌더링 묶음인 `BulletRenderer.SpriteBatch`, 각 이동 시퀀스의 `ISequenceRunner` 구현체가 있다. `BurstDataEditor.Spawn`은 미리보기 발사 일정을 보관한다. `Enemy.BurstYieldInstruntion`도 코드에 선언되어 있지만 `IEnumerator` 메서드가 `NotImplementedException`을 던지는 미완성 타입이다.
