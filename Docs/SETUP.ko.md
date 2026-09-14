# 개발 환경과 첫 실행

최신 차량·도로 변경과 실행법: [픽업트럭 물리](VEHICLE.ko.md), [외곽 순환도로와 접지 개선](ROADS.ko.md). 아래 초기 설정과 단계별 구현 기록을 함께 참고하세요.

## 확인한 기본 설정

- 실제 저장소는 바깥 폴더 안의 `NightCourier` 폴더입니다. `Assets`, `Packages`, `ProjectSettings`, `.git`이 함께 있는 위치에서 Git 명령을 실행합니다.
- 기존 브랜치: `main`. 원격: `https://github.com/TwShrimp/NightCourier.git`.
- Unity `6000.6.0f1`, URP `17.7.0`, Input System `1.20.0`.
- Asset Serialization은 Force Text, Version Control은 Visible Meta Files입니다.
- Active Input Handling은 새 Input System입니다. Both로 바꿀 필요가 없습니다.
- Git LFS를 설치하고 `git lfs install --local`로 이 저장소에 활성화했습니다.
- `core.autocrlf=false`는 이 저장소에만 설정했습니다. 텍스트 줄바꿈은 `.gitattributes`가 관리합니다.

## 변경 파일과 이유

| 파일 | 이유 |
| --- | --- |
| `.gitignore` | 기존 Unity 템플릿 보존, macOS 파일과 로컬 환경변수 파일 제외 |
| `.gitattributes` | 소스·씬·meta의 LF 통일, 모델·원본 이미지·오디오의 LFS 설정 |
| `.editorconfig` | 편집기 간 UTF-8, 들여쓰기, 줄바꿈 일치 |
| `Assets/_NightCourier/Scripts` | 입력, 주행, 카메라, 배달, 임시 도시 생성 책임 분리 |
| `Assets/_NightCourier/Scenes/DrivingPrototype.unity` | 기존 SampleScene과 분리한 실행 시작점 |
| `README.md`, `Docs` | GitHub 방문자용 설명, 개발 절차, 에셋 출처 기록 |

LFS 확장자 규칙은 소문자 기준입니다. 외부 파일은 소문자 확장자를 사용하세요. 작은 PNG/JPG는 현재 일반 Git에 저장하며 대용량 텍스처 도입 시 필요한 경로만 LFS에 추가합니다. 기존 커밋 기록은 재작성하지 않았습니다.

## Unity Editor 조작

1. Unity로 돌아와 import/compile이 끝날 때까지 기다립니다. 새 파일이 안 보이면 **Assets > Refresh**를 선택합니다.
2. Project 창에서 **Assets > _NightCourier > Scenes > DrivingPrototype**을 더블클릭합니다. 기존 씬 저장 질문이 나오면 본인이 수정한 내용이 있을 경우 저장합니다.
3. **Window > General > Console**을 열어 빨간 오류가 없는지 확인합니다.
4. 상단 **Play** 버튼을 누릅니다. 도시는 생성되지만 메인 화면에서 게임 시간과 오디오는 정지하고 주행 HUD는 숨겨집니다.
5. **차량 선택** 버튼이나 **V** 키를 누르면 회전하는 3D 미리보기를 보며 전기 픽업·2인승 스포츠카·택배 밴과 외장색 5종을 고를 수 있습니다. **선택**을 눌러 메인 화면으로 돌아오면 선택값이 저장되고 실제 주행 차량 외형에 반영됩니다.
6. **START!**를 클릭하거나 **Return**을 누르면 약 1.45초 동안 화면이 사라지고 소리가 올라오면서 차고에서 근무가 시작됩니다. **설정**에서는 영어·한국어를 전환할 수 있고 선택은 다음 실행에도 유지됩니다. 그래픽 품질은 현재 **MEDIUM**으로 고정되어 있습니다.
7. W/S 또는 위/아래 화살표로 전진·후진, A/D 또는 좌우 화살표로 조향합니다. Space는 브레이크, R은 시작 위치 복귀입니다. **C** 카메라는 약 0.52초 동안 다음 시점으로 부드럽게 이동합니다. **M**을 누르면 오른쪽 미니맵이 화면 중앙으로 부드럽게 확대됩니다. 확대 상태에서 마우스 왼쪽 버튼으로 지도를 끌어 이동하고, M을 다시 누르면 닫힙니다.
8. **Tab**을 누르면 게임 시간이 멈추고 일시정지 메뉴가 서서히 나타납니다. 위에서부터 **계속하기**, **설정**, **게임 종료** 순서입니다. 계속하기 또는 Tab을 다시 누르면 메뉴와 음량이 부드럽게 복귀합니다.
9. 먼저 물류창고의 청록색 적재 구역에서 정차해 소포 8개를 싣습니다. 이후 표시되는 배달 구역에서 속도 3 km/h 미만으로 머무르면 소포를 하나씩 전달합니다.
10. 여덟 번 배달하면 완료 문구가 나옵니다. Play를 껐다 켜면 새 경로를 시작합니다.

### 수동 검증

- 출발, 감속, 후진, 후진 시 반대 방향 조향이 자연스러운지 확인합니다.
- 정지 상태에서 A/D만 눌러 차가 제자리 회전하지 않는지 확인합니다.
- 건물·외곽 벽에 충돌하고 R로 복귀합니다.
- 배달 구역을 빠르게 통과하면 완료되지 않고, 정차하면 정확히 한 번 완료되는지 확인합니다.
- 여덟 배달 완료 후 추가 카운트가 없는지 확인합니다.
- Console에 오류가 없는지 확인합니다.

Unity 6000.6.0f1 Editor에서 Play 모드 회귀 테스트 15개로 메인 화면, 차량 선택과 저장, 시작·일시정지·재개 페이드, W 출발·D 조향·Space 제동·S 후진·R 복귀, 접지, 물류창고 적재와 배달 8회, 보도 진입, 고속 조향, 확장 도로, 배터리 충전, 부드러운 카메라 전환, 차량 조명과 전기 모터음, M 지도 확대와 도로 경로 계산을 검증합니다. 두 도시와 비는 Editor 화면에서도 확인했습니다. 플레이어 빌드와 장시간 성능 측정은 아직 하지 않았습니다.

## GitHub에 저장

현재 변경은 로컬 작업 파일입니다. 다음 명령으로 검토한 뒤 커밋하고 푸시할 수 있습니다.

```sh
git status
git diff --check
git add .
git diff --cached --stat
git commit -m "Add arcade driving and delivery prototype"
git push origin main
```

## Windows에서 이어서 작업

1. Git, Git LFS, Unity Hub와 동일한 Unity `6000.6.0f1`을 설치합니다.
2. 터미널에서 아래를 실행합니다. 첫 `git lfs install`은 새 컴퓨터의 사용자 설정입니다.

```sh
git lfs install
git clone https://github.com/TwShrimp/NightCourier.git
cd NightCourier
git config core.autocrlf false
git lfs pull
```

3. Unity Hub의 **Add > Add project from disk**에서 복제한 폴더를 선택합니다.
4. Unity가 Library를 새로 생성하도록 기다리고 위의 씬을 실행합니다.
5. 기기를 바꾸기 전에 작업한 기기에서 commit/push를 마치고, 다른 기기에서는 작업 트리가 깨끗한 상태에서 `git pull --ff-only`와 `git lfs pull`을 실행합니다. 충돌로 실패하면 강제 reset하지 말고 변경을 먼저 확인합니다.

폴더 이름을 대소문자만 바꿔야 할 때는 `git mv`로 임시 이름을 거쳐 변경하세요. `.meta`는 에셋과 함께 이동합니다.

## 빌드할 때

**File > Build Profiles**에서 원하는 플랫폼 프로필을 만들고, Scene List에 `DrivingPrototype`을 추가하고 실행할 씬으로 활성화합니다. SampleScene만 포함되어 있지 않은지 확인합니다. 플랫폼 모듈이 없다면 Unity Hub에서 해당 Editor의 모듈을 설치합니다. 이번 단계에서는 기존 빌드 설정을 변경하지 않았습니다.

공식 참고: [GitHub LFS 설정](https://docs.github.com/en/repositories/working-with-files/managing-large-files/configuring-git-large-file-storage), [Git LFS](https://git-lfs.com/).


## 도시 확장 및 WASD 수정

- `ArcadeCarController.cs`: 차체 아래에서 시작하던 접지 ray를 차체 하단보다 0.25m 위에서 시작하도록 변경했습니다. 도로 내부에서 ray가 시작되어 접지를 놓치는 문제를 방지하고 자기 Rigidbody는 제외합니다. 차량 전용 PhysicsMaterial의 마찰을 0으로 설정해 기본 차체 마찰이 작은 가속 증가분을 상쇄하지 않도록 했습니다. 횡방향 접지력과 제동은 컨트롤러가 담당합니다.
- `CarInput.cs`: Input System 자체의 포커스 처리를 사용하고 중복 `Application.isFocused` 조건을 제거했습니다.
- `World/CityBuilder.cs`: 720×600m 본도시의 56개 긴 블록, 4·6·8차선 도로망과 서로 다른 8종류의 건물 구성을 생성합니다. 차고·물류창고·충전소용 블록은 비워 두어 시설이 도로를 침범하지 않습니다.
- `World/CityGeometry.cs`: 장식은 블록과 재질별 메시로 합치며 건물·인도 등 실제 장애물에만 Collider를 만듭니다.
- `World/CityPalette.cs`: 외부 다운로드 없이 색상 팔레트와 아스팔트 노이즈 텍스처를 만듭니다.
- `World/WorldExpansionBuilder.cs`: 실내 조명이 있는 폐쇄형 차고, 물류창고, 본도시 간선도로와 직결된 670m 왕복 6차선 고속도로, 톨게이트, 확장 Harbor City와 전용 부지형 EV 충전망을 생성합니다.
- `Shaders/Resources/WorldSign.shader`: 글꼴 아틀라스의 알파를 사용해 간판을 그리며, 깊이 검사로 벽 너머 글자가 비치는 현상을 막습니다. Resources에 두어 빌드 시에도 포함되도록 했습니다.
- `World/NightAtmosphere.cs`: 비 입자, 안개, 야간 조명, URP Bloom/ACES/비네트와 한 번 갱신하는 반사 프로브를 구성합니다. 반사는 프로토타입 수준의 근사입니다.
- `PrototypeBootstrap.cs`: 도시 생성은 World로 옮기고 차량 조립과 씬 연결을 담당합니다. 자동차에 실제 바퀴, 빈 적재함과 8개의 가변 소포, 기능성 조명을 추가했습니다.
- `Prototype/MainMenuController.cs`: Play 직후 시간·오디오를 멈추고 HUD를 숨긴 상태에서 해상도에 맞춰 표시되는 시작·설정 화면을 구성합니다. 영어·한국어 선택을 저장하며, Start 입력 뒤 화면과 오디오를 함께 1.45초 동안 전환합니다.
- `ChaseCamera.cs`: 건물과 카메라 사이 장애물을 감지해 카메라가 벽을 통과하는 상황을 줄입니다.
- `DeliveryRoute.cs`, `CargoBedVisual.cs`, `DeliveryHud.cs`: 물류창고 적재, 도시 전역 8곳 배달, 소포 감소와 HUD를 분리했습니다. HUD에는 거리·배터리·미니맵·입력·접지 상태가 표시됩니다.
- `VehicleBattery.cs`, `EVChargingStation.cs`: 거리 기반 전력 소모, 방전 시 구동 제한과 저속 정차 충전을 담당합니다.
- `NightCourier.Runtime.asmdef`: URP 후처리 API 사용을 위해 Core/Universal Runtime 참조를 추가했습니다.
- `Tests`: Unity 상단 **NightCourier > Run Driving Regression Tests**로 접지·입력·출발·조향·제동·후진·복귀, 배달 8곳, 확장 월드, 배터리와 조명을 검증할 수 있습니다. 테스트는 Play 모드를 사용하므로 진행 중인 주행은 먼저 종료합니다.

실행 방법은 동일합니다. Play를 종료하고 컴파일이 끝난 뒤 `DrivingPrototype`에서 다시 Play를 누르세요. Game 화면 위에 마우스를 두고 **Shift+Space**로 크게 볼 수 있습니다. 입력 확인이 필요하면 Game 화면을 클릭하고 W를 누른 채 HUD 하단의 `Input +1`과 `ROAD CONTACT`를 확인합니다.

Unity 6.6이 Editor 실행 중 `QualitySettings.asset`의 직렬화 버전을 갱신하고 기본 `meshLodThreshold`를 추가했습니다. PC/Mobile 품질 값 자체는 변경하지 않았습니다.
