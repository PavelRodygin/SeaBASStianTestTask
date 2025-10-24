mergeInto(LibraryManager.library, {
  // Функция, которую вызовем из C#: Изменяет заголовок страницы
  SetPageTitle: function(newTitle) {
    var titleStr = UTF8ToString(newTitle);  // Конвертируем C#-строку в JS
    document.title = titleStr;
    console.log("Page title set to: " + titleStr);
  },

  // Функция для вызова Unity из JS: Мы вызовем её вручную для теста
  GetBrowserWidth: function() {
    var width = window.innerWidth;
    // Вызываем метод Unity (GameObject "GameManager", метод "ReceiveBrowserWidth")
    unityInstance.SendMessage('GameManager', 'ReceiveBrowserWidth', width);
  }
});