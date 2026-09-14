# 메뉴, 차량 선택과 카메라 전환

## 시작 화면 차량 선택

메인 화면의 **차량 선택**을 누르면 별도 카메라가 RenderTexture에 그리는 회전형 3D 미리보기가 열립니다. 차량은 기존 전기 픽업, 낮은 2인승 스포츠카, 높은 택배 밴 세 종류이며 외장색은 청록·검정·적색·은색·보라 5종입니다. **선택**을 누르면 메인 화면으로 돌아오고 차량 종류와 색이 `PlayerPrefs`에 저장됩니다. 다음 Play에서도 같은 외형으로 시작합니다.

세 차량은 바퀴 접점과 배달 진행 시스템을 공유하지만 주행 프로필은 분리되어 있습니다. 픽업은 200km/h 균형형 AWD, 스포츠카는 300km/h·520kW의 낮고 민첩한 GT, 밴은 160km/h의 무겁고 안정적인 화물 세팅입니다. 차종을 고르면 질량, 무게중심, 출력, 공기저항, 접지, 고속 조향, 충돌체와 휠 스킨이 함께 교체됩니다. 각 차의 전조등·후미등도 해당 외형의 자식으로 구성되어 선택하지 않은 차의 조명이 나타나지 않습니다.

## 일시정지

주행 중 **Tab**을 누르면 `Time.timeScale`이 즉시 0이 되어 차량과 배달 시간이 멈추고, 메뉴와 음량이 약 0.42초 동안 부드럽게 전환됩니다.

1. **계속하기**: 메뉴가 사라지고 음량이 복귀한 뒤 주행을 재개합니다. Tab을 다시 눌러도 같습니다.
2. **설정**: 시작 화면과 같은 영어·한국어 선택 및 `MEDIUM` 고정 그래픽 항목을 엽니다.
3. **게임 종료**: 빌드에서는 애플리케이션을 종료하고 Unity Editor에서는 Play 모드를 종료합니다.

## 카메라

**C**를 누르면 기존 네 시점 사이를 약 0.52초 동안 위치와 회전을 보간해 이동합니다. 전환할 때 마우스로 돌린 각도는 정면으로 초기화됩니다. 전환이 끝난 뒤 콕핏·본넷·앞바퀴 시점은 차량에 고정되며, 가속에 따른 뒤로 밀림은 3인칭 추적 시점에만 적용됩니다.

## 수정 파일

| 파일 | 역할 |
| --- | --- |
| `Scripts/Prototype/MainMenuController.cs` | 시작, 설정, 차량 선택, Tab 일시정지, 재개와 종료 흐름 및 화면·음량 페이드를 관리합니다. |
| `Scripts/Prototype/VehicleSelection.cs` | 선택 차량·색을 저장하고 실제 차량의 세 외형 중 하나를 활성화합니다. |
| `Scripts/Prototype/VehiclePreviewRenderer.cs` | 메뉴 전용 카메라·조명·RenderTexture와 세 가지 회전형 3D 미리보기를 생성합니다. |
| `Scripts/Prototype/PrototypeBootstrap.cs` | 픽업·스포츠카·밴 외형을 만들고 공통 물리·바퀴·조명·배달 시스템에 연결합니다. |
| `Scripts/Prototype/Vehicles/Common/VehicleLampRig.cs` | 방향지시등·브레이크·후진·전조등 상태를 공통 입력 시스템에 연결하는 조립 계층입니다. |
| `Scripts/Prototype/Vehicles/Pickup/PickupLightingBuilder.cs` | 픽업의 전폭 DRL, 원형 헤드램프와 가로형 후미등을 만듭니다. |
| `Scripts/Prototype/Vehicles/Sport/SportLightingBuilder.cs` | 스포츠카의 낮고 바깥으로 뻗는 이중 헤드램프와 분리형 테일 블레이드를 만듭니다. |
| `Scripts/Prototype/Vehicles/Van/VanLightingBuilder.cs` | 밴의 사각 프로젝터·상단 라이트 브로우와 세로형 후미등을 만듭니다. |
| `Scripts/Camera/ChaseCamera.cs` | C 입력 시 현재 카메라 자세에서 다음 마운트까지 위치·회전·FOV를 부드럽게 보간합니다. |

Unity에서는 Play 후 차량별 3D 미리보기와 색을 차례로 선택하고 **선택 → START!**로 실제 외형이 일치하는지 확인하세요. 주행 중 Tab 메뉴의 세 버튼, 설정의 언어 공유, 재개 페이드와 C 전환을 확인하면 됩니다.
