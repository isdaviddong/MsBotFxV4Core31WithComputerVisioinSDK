# Bot Framework V4 結合 Computer Vision SDK 範例專案

這個專案展示了如何使用 Bot Framework V4 建立一個可以辨識圖片內容的聊天機器人。機器人使用 Azure 的 Computer Vision 服務來分析上傳的圖片。

## 主要功能

- 接收使用者上傳的圖片
- 使用 Computer Vision API 分析圖片內容
- 辨識圖片中的人臉並標示年齡與性別
- 將處理後的圖片上傳至 Imgur 並回傳結果
<img src='https://i.imgur.com/E3RNYfS.jpg' />

## 專案結構

### EchoBot.cs
主要的機器人邏輯處理類別，包含：
- 處理使用者訊息的 `OnMessageActivityAsync` 方法
- 處理圖片附件並呼叫 Computer Vision 服務
- 設定 Computer Vision 的端點和金鑰

### ComputerVisionHelper.cs
Computer Vision 相關功能的輔助類別，包含：
- `MakeRequest`: 直接呼叫 Computer Vision REST API
- `ProcessImage`: 主要的圖片處理邏輯
  - 辨識圖片內容描述
  - 偵測人臉位置
  - 標示年齡和性別
  - 在圖片上繪製標記
- `UploadImage2Imgur`: 將處理後的圖片上傳至 Imgur

## 使用設定

使用前需要設定以下參數：

1. 在 EchoBot.cs 中設定：
```csharp
static string endpoint = "https://YOUR_COMPUTER_VISION_ENDPOINT.cognitiveservices.azure.com/";
static string subscriptionKey = "YOUR_COMPUTER_VISION_KEY";
```

2. 在 ComputerVisionHelper.cs 中設定：
```csharp
var clientId = "YOUR_IMGUR_CLIENT_ID";
```

## 使用流程

1. 啟動機器人後，會顯示歡迎訊息「請上傳圖片」
2. 使用者上傳圖片後，機器人會：
   - 分析圖片內容
   - 標示人臉（如果有的話）
   - 顯示年齡和性別統計
   - 回傳處理後的圖片連結

## 回應格式

機器人會回傳以下資訊：
- 圖片的內容描述
- 偵測到的人臉數量及性別分布
- 處理後圖片的 Imgur 連結

## 注意事項

- 需要有效的 Azure Computer Vision 服務訂閱
- 需要有效的 Imgur API 客戶端 ID
- 圖片處理可能需要一些時間，請耐心等待
 