# 코드베이스 안내서

## 목차

1. [프로젝트 개요와 폴더 구조](#1-프로젝트-개요와-폴더-구조)
2. [아키텍처: 주요 시스템의 역할과 데이터 흐름](#2-아키텍처-주요-시스템의-역할과-데이터-흐름)
3. [데이터 에셋: ScriptableObject 계층과 필드](#3-데이터-에셋-scriptableobject-계층과-필드)
4. [클래스: 책임과 주요 공개 메서드](#4-클래스-책임과-주요-공개-메서드)

## 1. 프로젝트 개요와 폴더 구조

Danmaku는 Unity 6로 만든 2D 탄막 게임이다. 게임 로직은 `Assets/02.Scripts/`에 있으며, 에셋 구성 데이터를 `ScriptableObject`로 저장하고 실행 중에는 `StageRunner`, `EnemyManager`, `BulletManager` 등이 이를 처리한다. 게임 시간 진행과 이동은 주로 `FixedUpdate` 및 `Time.fixedDeltaTime`을 사용한다. `WaitForFixedFrame`은 지정한 수의 고정 프레임을 기다리는 코루틴 대기 도구다.

| 경로 | 포함 내용 |
|---|---|
| `Assets/02.Scripts/Stage/` | 스테이지와 페이즈 데이터, 적 출현 순서, 스테이지 실행 |
| `Assets/02.Scripts/Enemy/` | 적 데이터와 출현 패턴, 적 및 보스 동작 |
| `Assets/02.Scripts/Spell/` | 스펠 카드, 탄막 묶음, 탄환 데이터 및 시퀀스 |
| `Assets/02.Scripts/Spell/Burst/` | 버스트 구성과 탄환 초기 상태 |
| `Assets/02.Scripts/Spell/Bullet/` | 런타임 탄환, 풀 관리, 충돌 격자, 탄환 종류와 태그 |
| `Assets/02.Scripts/Spell/Bullet/Sequence/` | 탄환 이동 시퀀스 구현 |
| `Assets/02.Scripts/Managers/` | 적·탄환·게임·상태·UI 관리자와 탄환 렌더러 |
| `Assets/02.Scripts/Input/` | 입력 이벤트와 화면 스와이프 처리 |
| `Assets/02.Scripts/Player/` | 플레이어 이동 및 애니메이션 |
| `Assets/02.Scripts/UI/` | 누름·탭 방식의 UI 입력 컴포넌트 |
| `Assets/02.Scripts/Core/` | 싱글턴 기반 클래스, 고정 프레임 대기, 최고 점수 저장 |
| `Assets/02.Scripts/Editor/` | 데이터 프리뷰, 데드존 표시, 실행·UI 테스트용 인스펙터 |
| `Assets/02.Scripts/Test/` | 스펠 카드 버스트를 시험하는 `BulletTest` |

## 2. 아키텍처: 주요 시스템의 역할과 데이터 흐름

### 스테이지와 적 출현

`StageData`는 페이즈 배열을 보유한다. `StageRunner.Initialize`로 실행 데이터를 지정하고 `StartRun`으로 코루틴을 시작하면, 각 페이즈의 `WaitForFirstEmenyFrame`만큼 기다린 뒤 `EnemyChains`와 각 체인의 `EnemyPatterns`를 순서대로 처리한다. 각 패턴은 `EnemyManager.SpawnEnemy`에 전달되고, 체인의 각 적 출현 사이에는 `WaitForNextEmenyFrame`만큼 기다린다.

`PhaseData.Boss`도 `EnemyPattern`으로 저장되지만, 현재 `StageRunner` 실행 흐름은 이 필드를 읽지 않는다. `StageData.StageTitle`과 `BGM` 또한 데이터에는 있으나 `StageRunner`에서 사용하지 않는다.

### 적과 스펠 카드

`EnemyManager`는 `EnemyPattern.EnemyData.Type`이 `Common`이면 풀에서 일반 적을 가져오고, 그 외의 값이면 `EnemyBoss` 오브젝트를 생성한다. 적을 초기화할 때 출현 위치와 적 색상을 적용한다.

`Enemy.StartPattern`은 `EnemyData.Spells`를 차례로 실행한다. 각 스펠의 `PresentHP`를 적 HP로 설정하고 `BulletManager.BurstChain`에 스펠의 버스트 목록과 적의 `Transform`을 전달한다. 버스트 코루틴이 끝나면 스펠의 시간 제한과 남은 HP를 확인해 반복 여부를 결정하고, 스펠 목록이 끝나면 `OnDie`를 호출한다. `EnemyBoss`는 사망 처리에서 즉시 반환하지 않고 위쪽으로 이동하는 코루틴을 마친 뒤 부모 `OnDie`에 처리를 맡긴다.

### 버스트와 탄환 생성

`BulletManager.BurstChain`은 `BurstData`를 순서대로 읽는다. 각 버스트에서 `BulletPatterns`를 차례로 실행해 탄환을 만들고, 각 `BulletPattern.WaitForFrame`만큼 기다린다. 버스트의 탄환 생성이 끝나면 `BurstData.WaitForFrame`만큼 기다린 뒤 다음 버스트로 넘어간다.

탄환 생성 시 `Bullet.Initialize`는 `BulletData`, `InitialBullet`, `BulletSequence`를 적용한다. 초기 위치에는 전달받은 발사 주체(`presenter`)의 위치를 더한다. 여기서 `BurstData.PresentPosition`은 저장 필드이지만 이 생성 흐름에서는 참조하지 않는다.

### 탄환과 이동 시퀀스

`Bullet`은 `GameObject`가 아닌 C# 객체이며 `BulletManager`의 `ObjectPool<Bullet>`로 재사용된다. 매 고정 프레임마다 활성 탄환에 대해 시퀀스 실행자의 `Next`를 호출하고, 데드존 밖 탄환은 풀에 반환한다. 활성 목록은 순서를 유지하며 제자리에서 압축된다.

`BulletSequenceBase`는 `ISequenceRunner`를 생성한다. `DefaultSequence`는 속도와 토크로 이동 방향을 회전시키고, `AccelateSequence`는 커브 보간 후 가속도를 적용하며, `TargettingSequence`는 시작 시 플레이어 방향으로 속도를 설정한다. `SequenceBridge`는 여러 시퀀스 실행자를 프레임 간격으로 이어 실행한다. 시퀀스 이동은 `Time.fixedDeltaTime`을 기준으로 한다.

### 렌더링과 충돌 판정

`BulletRenderer`는 `BulletManager.ActiveBullets`를 읽어 스프라이트별 메시와 인스턴스를 묶은 뒤 `Graphics.DrawMeshInstanced`로 렌더링한다. 드로우 호출당 최대 1023개를 처리하고, 탄환의 색과 진행 방향을 그리기에 반영한다. 별도 소스가 설정되지 않으면 `BulletManager`의 활성 목록을 찾는다. 렌더러는 탄환 데이터를 변경하지 않는다.

`BulletCollision`은 탄환 태그별 균일 격자를 만들고 원형 탄환 및 레이저와 원형 엔티티의 겹침을 조회하는 정적 도구다. `BulletManager`는 매 `FixedUpdate`에서 탄환 이동·정리를 마친 뒤 격자를 `Rebuild`한다. `Check`와 `CheckAll` 조회 메서드는 제공되지만, 이 디렉터리의 플레이어·적 동작 코드에서는 조회 결과를 사용하지 않는다.

### 상태와 UI

`StatusManager`는 HP, 파워, 현재 점수, 최고 점수를 보유하고 변경 이벤트를 발생시킨다. 최고 점수는 `DataSaveLoad`를 통해 `PlayerPrefs`에 저장된다. 피격은 18 고정 프레임 뒤 HP를 줄이며, 그 대기 중 `TryUsePower` 또는 `TryUseSemiPower`가 성공하면 대기 중인 피격 처리를 취소한다.

`UIManager`는 HP·파워 게이지, 점수 텍스트, 보스 UI를 갱신하는 메서드를 제공한다. `StatusManager`는 변경 통지를 `UnityEvent` 필드로 노출하며, 해당 이벤트의 연결은 컴포넌트의 직렬화된 이벤트 설정에 맡긴다. `StatusManager`는 자신이 `UIManager`의 자식이 아닌 경우 오브젝트를 제거한다.

### 입력과 플레이어

`InGameInputHandler`는 이동, 폭탄 입력, 저속 이동 상태, 스와이프 이벤트를 제공한다. 입력 액션 콜백은 `InvokeOnMove`, 두 `ChangeSlowMoveState` 오버로드, `InvokeOnUseBomb`에서 이벤트 또는 상태로 전달된다. `HoldButton`과 `TapButton`은 각각 포인터 누름 상태와 탭 이벤트를 `UnityEvent`로 내보내며, `InGameSwipe`는 포인터 이동 거리가 설정 기준 이상일 때 스와이프 벡터를 입력 처리기에 전달한다.

`PlayerController`는 이동 이벤트로 이동 벡터를 받고, 매 고정 프레임에 저속 이동 여부를 반영해 이동한 뒤 설정된 데드존 안으로 위치를 제한한다. 폭탄 이벤트는 `Bomb` 메서드에 연결되어 있지만 메서드 본문은 비어 있다. `PlayerAnimation`은 이동 이벤트의 수평 방향에 따라 Animator의 `Right` 정수 매개변수를 `-1`, `0`, `1`로 설정한다.

## 3. 데이터 에셋: ScriptableObject 계층과 필드

데이터 구성은 다음과 같다. `EnemyChain`, `EnemyPattern`, `BulletPattern`, `InitialBullet`은 직렬화 가능한 값 형식이며, 그 외의 단계별 구성 요소는 `ScriptableObject` 참조로 연결된다.

`StageData` → `PhaseData` → `EnemyChain` → `EnemyPattern` → `EnemyData` → `SpellCard` → `BurstData` → `BulletPattern`

| 데이터 형식 | 직렬화 필드 | 의미와 코드에서의 사용 |
|---|---|---|
| `StageData` | `StageTitle` (`string`) | 스테이지 제목 |
|  | `BGM` (`AudioClip`) | 스테이지 배경 음악 에셋 참조. 현재 `StageRunner`는 재생하지 않는다. |
|  | `Phases` (`PhaseData[]`) | 스테이지 순서대로 처리할 페이즈 |
| `PhaseData` | `EnemyChains` (`EnemyChain[]`) | 페이즈 안에서 순서대로 처리하는 적 출현 체인 |
|  | `WaitForFirstEmenyFrame` (`int`) | 해당 페이즈 시작 시 적 출현 전에 기다리는 고정 프레임 수 |
|  | `Boss` (`EnemyPattern`) | 보스 출현 패턴 데이터. 현재 `StageRunner`에서는 사용하지 않는다. |
| `EnemyChain` | `EnemyPatterns` (`EnemyPattern[]`) | 순서대로 출현시킬 적 패턴 |
|  | `WaitForNextEmenyFrame` (`int`) | 패턴 처리 후 다음 적 패턴 전에 기다리는 고정 프레임 수 |
| `EnemyPattern` | `EnemyData` (`EnemyData`) | 출현 적의 종류·표시 정보와 스펠 목록 |
|  | `SpawnPosition` (`Vector2`) | 적의 초기 위치 |
|  | `WaitForFirstBurstFrame` (`int`) | 적 스펠 탄막 시작 전 대기 프레임 수. `Enemy`가 각 스펠 처리 전에 이 값을 확인한다. |
| `EnemyData` | `Type` (`EnemyType`) | 적 유형. `EnemyManager`는 `Common`은 풀에서 처리하고 다른 값은 보스로 생성한다. |
|  | `EnemyName` (`string`) | 적 이름 |
|  | `Description` (`string`) | 적 설명 텍스트 |
|  | `EnemyColor` (`Color`) | 적 `SpriteRenderer`에 적용할 색상 |
|  | `spells` (`SpellCard[]`) | 적이 처리할 스펠 카드. 공개 읽기 전용 뷰는 `Spells`다. |
| `SpellCard` | `SpellName` (`string`) | 스펠 카드 이름 |
|  | `SpellDescription` (`string`) | 스펠 카드 설명 |
|  | `PresentHP` (`int`) | 스펠 실행 때 적 HP로 설정하는 값 |
|  | `TimeLimit` (`int`) | `Enemy`의 스펠 탄막 반복 조건에서 확인하는 시간 제한 값 |
|  | `bursts` (`BurstData[]`) | 순서대로 실행할 버스트. 공개 열거 뷰는 `Bursts`다. |
| `BurstData` | `bulletPatterns` (`BulletPattern[]`) | 버스트에서 순서대로 생성할 탄환 패턴. 공개 읽기 전용 뷰는 `BulletPatterns`다. |
|  | `PresentPosition` (`Vector2`) | 버스트 데이터에 보관되는 위치 값. 현재 `BulletManager`의 생성 처리에서는 사용하지 않는다. |
|  | `WaitForFrame` (`int`) | 버스트 내 탄환 생성이 끝난 뒤 다음 버스트 전에 기다릴 고정 프레임 수 |
| `BulletPattern` | `BulletData` (`BulletData`) | 탄환 렌더링·종류·태그·판정 크기 데이터 |
|  | `InitialBullet` (`InitialBullet`) | 생성 시 색상, 위치, 속도, 토크 |
|  | `BulletSequence` (`BulletSequenceBase`) | 탄환 이동 규칙을 제공하는 시퀀스 데이터 |
|  | `WaitForFrame` (`int`) | 이 탄환 생성 후 다음 패턴을 처리하기 전에 기다릴 고정 프레임 수 |
| `InitialBullet` | `BulletColor` (`Color`) | 렌더링할 탄환 색상 |
|  | `InitialPosition` (`Vector2`) | 발표자 위치를 기준으로 한 탄환 초기 위치 |
|  | `InitialVelocity` (`Vector2`) | 탄환 초기 속도 벡터 |
|  | `InitialTorqueDegree` (`float`) | 탄환 이동 방향 회전에 쓰는 토크 값(도 단위); 초기화 때 라디안으로 변환된다. |
| `BulletData` | `Type` (`BulletType`) | 판정 형식. `Circle` 또는 `Laser` |
|  | `Tag` (`BulletTag`) | 격자에서 구분하는 탄환 발사 주체. `Enemy` 또는 `Player` |
|  | `BulletImage` (`Sprite`) | 렌더러에서 사용할 스프라이트 |
|  | `Distance` (`float`) | `Circle`에서는 반지름, `Laser`에서는 레이저 길이로 충돌 판정에 사용된다. |

### 이동 시퀀스 에셋의 추가 설정값

이동 시퀀스 에셋은 `BulletPattern.BulletSequence`에서 참조한다. 아래 설정은 `BulletPattern` 자체 필드는 아니다.

| 형식 | 설정 필드 | 의미 |
|---|---|---|
| `AccelateSequence` | `TargetVelocity` (`float`) | `Duration` 동안 커브로 보간할 목표 속력 |
|  | `Curve` (`AnimationCurve`) | 시작 속력에서 목표 속력까지의 보간 값 |
|  | `Duration` (`int`) | 커브 보간 기간. 0이면 처음부터 가속도 계산으로 진행한다. |
|  | `Acceleration` (`float`) | 기간 이후 적용하는 초당 속력 변화량 |
| `SequenceBridge` | `sequences` (`BulletSequenceBase[]`) | 연결할 이동 시퀀스 에셋 |
|  | `frameBetweenSequence` (`int[]`) | 다음 시퀀스로 넘어갈 때까지의 프레임 기준 값 |
| `DefaultSequence` | 별도 직렬화 설정 없음 | 초기 속도와 토크로 이동을 계산한다. |
| `TargettingSequence` | 별도 직렬화 설정 없음 | 실행 시작 시 찾은 `PlayerController`의 위치를 향하도록 속도를 설정한다. |

## 4. 클래스: 책임과 주요 공개 메서드

### 스테이지 및 적

| 형식과 파일 | 책임 및 주요 공개 멤버 |
|---|---|
| `StageData` — `Assets/02.Scripts/Stage/StageData.cs` | 스테이지 제목, BGM, 페이즈 참조를 보유한다. 읽기 전용 프로퍼티는 `StageTitle`, `BGM`, `Phases`다. |
| `PhaseData` — `Assets/02.Scripts/Stage/PhaseData.cs` | 적 체인, 시작 대기 프레임, 보스 패턴 데이터를 보유한다. 읽기 전용 프로퍼티는 `EnemyChains`, `WaitForFirstEmenyFrame`, `Boss`다. |
| `EnemyChain` — `Assets/02.Scripts/Stage/EnemyChain.cs` | 적 패턴 배열과 패턴 사이 대기 프레임을 묶는 직렬화 구조체다. `EnemyPatterns`, `WaitForNextEmenyFrame`를 제공한다. |
| `StageRunner` — `Assets/02.Scripts/Stage/StageRunner.cs` | 스테이지 순서 실행 컴포넌트다. `Initialize(StageData)`로 데이터를 지정하고 `StartRun()`으로 실행한다. |
| `EnemyPattern` — `Assets/02.Scripts/Enemy/EnemyPattern.cs` | 적 데이터, 출현 위치, 첫 버스트 전 대기 프레임을 묶는 직렬화 구조체다. |
| `EnemyData` — `Assets/02.Scripts/Enemy/EnemyData.cs` | 적의 유형·이름·설명·색상 및 스펠 목록을 보유한다. 공개 스펠 접근자는 `Spells`다. |
| `EnemyType` — `Assets/02.Scripts/Enemy/EnemyType.cs` | 적 유형 열거형이다: `Common`, `MiddleBoss`, `LastBoss`. |
| `Enemy` — `Assets/02.Scripts/Enemy/Enemy.cs` | 적 컴포넌트다. `Initialize(EnemyPattern)`은 위치와 색상 등 초기 상태를 적용하고, `StartPattern()`은 스펠과 버스트 실행을 시작하며, `SendDamage(int)`는 HP를 감소시킨다. 파생 클래스가 `OnDie()`를 재정의할 수 있다. |
| `EnemyBoss` — `Assets/02.Scripts/Enemy/EnemyBoss.cs` | `Enemy`를 상속하며 사망 시 퇴장 이동 후 기본 사망 처리를 호출한다. 별도 공개 메서드는 추가하지 않는다. |
| `EnemyManager` — `Assets/02.Scripts/Managers/EnemyManager.cs` | 일반 적 풀과 보스 생성을 관리한다. `SpawnEnemy(EnemyPattern)`으로 유형에 따라 생성하고 `ReleaseEnemy(Enemy)`로 일반 적을 풀에 반환하거나 보스를 제거한다. |

### 스펠과 탄환

| 형식과 파일 | 책임 및 주요 공개 멤버 |
|---|---|
| `SpellCard` — `Assets/02.Scripts/Spell/SpellCard.cs` | 스펠 이름·설명·HP·시간 제한과 버스트 목록을 보유한다. 버스트 접근자는 `Bursts`다. |
| `BurstData` — `Assets/02.Scripts/Spell/Burst/BurstData.cs` | 탄환 패턴 목록, 위치 데이터, 버스트 후 대기 시간을 보유한다. 공개 접근자는 `PresentPosition`, `WaitForFrame`, `BulletPatterns`다. |
| `BulletPattern` — `Assets/02.Scripts/Spell/Burst/BulletPattern.cs` | 탄환 데이터, 초기 상태, 이동 시퀀스, 패턴 간격을 묶는 직렬화 구조체다. |
| `InitialBullet` — `Assets/02.Scripts/Spell/Burst/InitialBulletData.cs` | 탄환 초기 색상·위치·속도·토크를 묶는 직렬화 구조체다. |
| `BulletData` — `Assets/02.Scripts/Spell/Bullet/BulletData.cs` | 탄환의 종류·태그·스프라이트·판정 거리를 보유한다. 공개 읽기 전용 프로퍼티는 `Type`, `Tag`, `BulletImage`, `Distance`다. |
| `BulletType` — `Assets/02.Scripts/Spell/Bullet/BulletType.cs` | 판정 형식 열거형이다: `Circle`, `Laser`. |
| `BulletTag` — `Assets/02.Scripts/Spell/Bullet/BulletTag.cs` | 탄환 발사 주체 열거형이다: `Enemy`, `Player`. |
| `Bullet` — `Assets/02.Scripts/Spell/Bullet/Bullet.cs` | 풀링되는 런타임 탄환 데이터 객체다. `Initialize(BulletPattern, Vector2)`로 패턴을 적용하고, `Start()`와 `Update()`로 시퀀스 실행자를 진행하며, `Clear()`로 재사용 상태를 초기화한다. |
| `BulletManager` — `Assets/02.Scripts/Spell/Bullet/BulletManager.cs` | 탄환 풀, 활성 목록, 데드존, 버스트 실행을 관리한다. `SpawnBullet(BulletPattern, Transform)`은 한 발을 생성하고, `BurstChain(IEnumerable<BurstData>, Transform)`은 버스트 코루틴을 시작한다. `ActiveBullets`는 활성 목록의 읽기 전용 접근자다. |
| `BulletCollision` — `Assets/02.Scripts/Spell/Bullet/BulletCollision.cs` | 정적 충돌 격자다. `Rebuild(IReadOnlyList<Bullet>, Rect, float)`로 격자를 만들고, `Check(Vector2, BulletTag, float)` 또는 `Check(Vector2, BulletTag, float, out Bullet)`로 첫 충돌을 확인한다. `CheckAll(Vector2, BulletTag, float, List<Bullet>)`은 겹친 탄환을 결과 목록에 채운다. `DefaultCellSize`는 기본 칸 크기다. |

### 탄환 이동 시퀀스

| 형식과 파일 | 책임 및 주요 공개 멤버 |
|---|---|
| `BulletSequenceBase` — `Assets/02.Scripts/Spell/Bullet/Sequence/BulletSequenceBase.cs` | 시퀀스 에셋의 추상 기반 클래스다. `GetSequenceRunner()`는 실행자를 반환한다. 중첩 인터페이스 `ISequenceRunner`는 `Start(Bullet)`과 `Next(Bullet)`을 정의한다. |
| `DefaultSequence` — `Assets/02.Scripts/Spell/Bullet/Sequence/DefaultSequence.cs` | 초기 속도에 따라 이동하고 토크로 속도 방향을 회전하는 기본 시퀀스다. `GetSequenceRunner()`를 구현한다. |
| `AccelateSequence` — `Assets/02.Scripts/Spell/Bullet/Sequence/AccelateSequence.cs` | 설정 커브와 기간에 따른 속력 보간 후 가속을 적용한다. `GetSequenceRunner()`를 구현하고 `TargetVelocity`, `Curve`, `Duration`, `Acceleration` 프로퍼티를 제공한다. |
| `TargettingSequence` — `Assets/02.Scripts/Spell/Bullet/Sequence/TargettingSequence.cs` | 실행 시작 시 `PlayerController`를 찾아 탄환 속도를 플레이어 방향으로 설정한다. `GetSequenceRunner()`를 구현한다. |
| `SequenceBridge` — `Assets/02.Scripts/Spell/Bullet/Sequence/SequenceBridge.cs` | 여러 시퀀스를 프레임 기준으로 연결한다. `GetSequenceRunner()`를 구현한다. |

### 관리자, 렌더링 및 사용자 인터페이스

| 형식과 파일 | 책임 및 주요 공개 멤버 |
|---|---|
| `BulletRenderer` — `Assets/02.Scripts/Managers/BulletRenderer.cs` | 활성 탄환을 스프라이트별 GPU 인스턴싱으로 렌더링한다. `SetSource(IReadOnlyList<Bullet>)`로 사용할 탄환 목록을 지정하고, `BulletMaterial`, `Depth`, `RotateToVelocity` 프로퍼티로 렌더링 설정을 제공한다. |
| `GameManager` — `Assets/02.Scripts/Managers/GameManager.cs` | `SingletonBehaviour<GameManager>` 기반 관리자다. `WaitForFixedUpdate`를 공유 대기 객체로 제공하고 `FrameToSecond(int)`로 프레임 수를 초 단위 값으로 환산한다. |
| `StatusManager` — `Assets/02.Scripts/Managers/StatusManager.cs` | 플레이어 상태, 점수, 이벤트, 피격 및 파워 사용을 관리한다. `Initialize(int, decimal)`, `AddHP(int)`, `AddPower(decimal)`, `AddScore(int)`, `Hit()`, `TryUsePower()`, `TryUseSemiPower()`를 제공한다. `MAX_HP`와 `MAX_POWER`는 최대 표시 기준 상수다. |
| `UIManager` — `Assets/02.Scripts/Managers/UIManager.cs` | 플레이어 상태와 보스 UI의 표시를 갱신한다. `SetHPSlider(int)`, `SetPowerSlider(decimal)`, `SetScoreText(int)`, `SetHighScoreText(int)`, `SetBossName(string)`, `SetBossHP(int, int)`를 제공한다. |

### 입력, 플레이어, 공통 도구 및 테스트

| 형식과 파일 | 책임 및 주요 공개 멤버 |
|---|---|
| `InGameInputHandler` — `Assets/02.Scripts/Input/InGameInputHandler.cs` | 입력 이벤트 및 저속 이동 상태를 제공한다. `OnMove`, `OnUseBomb`, `OnSwipe` 이벤트와 `IsSlowMoving` 상태를 노출하며, `InvokeOnMove`, `InvokeOnUseBomb`, 두 `ChangeSlowMoveState` 오버로드, `Swipe`로 입력을 전달한다. |
| `InGameSwipe` — `Assets/02.Scripts/Input/InGameSwipe.cs` | 포인터 시작점과 끝점의 거리를 계산해 기준을 넘으면 `InGameInputHandler.Swipe`를 호출한다. 이벤트 시스템 메서드 `OnPointerDown` 및 `OnPointerUp`을 구현한다. |
| `PlayerController` — `Assets/02.Scripts/Player/PlayerController.cs` | 입력 이동 이벤트를 구독하고 고정 프레임마다 플레이어를 이동·제한한다. 설정 프로퍼티는 `Speed`, `LowSpeedMagnification`, `Radius`, `DeadzoneMinTr`, `DeadzoneMaxTr`다. |
| `PlayerAnimation` — `Assets/02.Scripts/Player/PlayerAnimation.cs` | 이동 이벤트의 좌우 방향을 Animator `Right` 매개변수에 반영한다. |
| `HoldButton` — `Assets/02.Scripts/UI/HoldButton.cs` | 포인터 누름·해제 때 `OnChangeTouchState`의 `UnityEvent<bool>`에 참·거짓을 전달한다. `OnPointerDown`, `OnPointerUp`을 구현한다. |
| `TapButton` — `Assets/02.Scripts/UI/TapButton.cs` | 포인터 클릭 때 `OnTap`의 `UnityEvent`를 호출한다. `OnPointerClick`을 구현한다. |
| `SingletonBehaviour<T>` — `Assets/02.Scripts/Core/SingletonBehaviour.cs` | 관리자 계열의 싱글턴 기반 클래스다. `Instance`로 인스턴스를 제공하며, 상속 클래스는 `Awake()`를 재정의할 때 기반 메서드를 호출해야 싱글턴 등록이 수행된다. |
| `WaitForFixedFrame` — `Assets/02.Scripts/Core/WaitForFixedFrame.cs` | `IEnumerator` 기반 고정 프레임 대기 도구다. 생성자 `WaitForFixedFrame(int)`에 대기 프레임 수를 전달하고 `MoveNext`, `Current`, `Reset`으로 코루틴 대기 상태를 처리한다. |
| `DataSaveLoad` — `Assets/02.Scripts/Core/DataSaveLoad.cs` | `PlayerPrefs`를 통해 최고 점수를 저장하고 읽는다. 정적 프로퍼티 `HighScore`를 제공한다. |
| `BulletTest` — `Assets/02.Scripts/Test/BulletTest.cs` | 인스펙터의 `SpellCard[]`를 대상으로 각 스펠의 `Bursts`를 현재 오브젝트 위치에서 실행한다. 공개 테스트 메서드는 `SpellOneShot()`이다. |

### 에디터 도구

| 형식과 파일 | 책임 및 주요 공개 메서드 |
|---|---|
| `BulletDataEditor` — `Assets/02.Scripts/Editor/BulletDataEditor.cs` | `BulletData` 인스펙터를 그린다. `OnInspectorGUI()`에서 데이터를 편집하고 스프라이트와 충돌 모양 미리보기를 표시한다. |
| `DeadzoneGizmos` — `Assets/02.Scripts/Editor/DeadzoneGizmos.cs` | 플레이어와 탄환 데드존을 씬 뷰에 표시하는 정적 편집기 도구다. 내부 그리기 메서드는 `PlayerController`와 `BulletManager`의 두 경계 트랜스폼을 읽는다. |
| `StageRunEditor` — `Assets/02.Scripts/Editor/StageRunEditor.cs` | 플레이 모드에서 `StageRunner` 인스펙터에 시작 버튼을 표시한다. `OnInspectorGUI()`에서 버튼 입력 시 `StartRun()`을 호출한다. |
| `UIManagerEditor` — `Assets/02.Scripts/Editor/UIManagerEditor.cs` | `UIManager` 인스펙터에 HP·파워·점수·보스 UI 시험 패널을 추가한다. `OnInspectorGUI()`에서 테스트 값을 적용한다. |
