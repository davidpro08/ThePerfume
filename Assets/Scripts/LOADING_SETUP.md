# 로딩 창 설정 가이드

## 필수 설정 사항

### 1. 프리팹 복사

Unity 에디터에서 다음 작업을 수행해주세요:

1. `Assets/Prefab/UI/LoadingUI.prefab` 파일을 선택합니다
2. `Ctrl+C`로 복사합니다
3. `Assets/Resources/UI/` 폴더로 이동합니다
4. `Ctrl+V`로 붙여넣습니다
5. 프리팹 이름이 `LoadingUI`인지 확인합니다

### 2. LoadingUI 프리팹 설정

프리팹에 다음 컴포넌트들이 설정되어 있어야 합니다:

#### 필수 컴포넌트:

- **CanvasGroup** (`_loadingCanvas`)
  - Alpha: 0 (초기값)
  - Blocks Raycasts: false (초기값)
- **Image** (`_progressBar`)
  - Image Type: Filled
  - Fill Method: Horizontal
  - Fill Amount: 0 (초기값)

#### 선택적 컴포넌트:

- **TextMeshProUGUI** (`_loadingText`) - 로딩 메시지 표시
- **TextMeshProUGUI** (`_progressText`) - 진행률 퍼센트 표시 (예: "50%")

### 3. 프리팹 구조 예시

```
LoadingUI (GameObject)
├── Canvas
│   ├── CanvasGroup (컴포넌트)
│   ├── Background (Image - 검은색 반투명 배경)
│   ├── LoadingPanel
│   │   ├── ProgressBar (Image - 진행바)
│   │   ├── LoadingText (TextMeshProUGUI - "로딩 중...")
│   │   └── ProgressText (TextMeshProUGUI - "0%")
```

### 4. 사용 방법

```csharp
// 씬 전환 시 로딩 창 표시
LoadingUIManager.Instance.LoadScene("SceneName");
```

## 문제 해결

### 프리팹을 찾을 수 없다는 에러가 발생하는 경우:

1. `Assets/Resources/UI/LoadingUI.prefab` 파일이 존재하는지 확인
2. 프리팹 이름이 정확히 `LoadingUI`인지 확인
3. Unity 에디터에서 Resources 폴더를 새로고침 (Ctrl+R)

### 로딩 창이 표시되지 않는 경우:

1. 프리팹에 `LoadingUIManager` 컴포넌트가 추가되어 있는지 확인
2. CanvasGroup, ProgressBar 등 필수 컴포넌트가 할당되어 있는지 확인
3. Canvas의 Render Mode가 Screen Space - Overlay인지 확인

