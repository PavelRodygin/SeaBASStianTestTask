mergeInto(LibraryManager.library, {
  InitAppEvents: function(gameObjectNamePtr) {
    var gameObjectName = UTF8ToString(gameObjectNamePtr);
    
    console.log('[AppEvents] Initializing for GameObject: ' + gameObjectName);
    
    // Событие фокуса (окно активно)
    window.addEventListener('focus', function() {
      console.log('[AppEvents] Focus gained');
      SendMessage(gameObjectName, 'OnFocus', 1);
    });

    // Событие потери фокуса (окно неактивно)
    window.addEventListener('blur', function() {
      console.log('[AppEvents] Focus lost');
      SendMessage(gameObjectName, 'OnFocus', 0);
    });

    // Событие изменения видимости вкладки
    document.addEventListener('visibilitychange', function() {
      var isHidden = document.hidden;
      console.log('[AppEvents] Visibility changed, hidden: ' + isHidden);
      SendMessage(gameObjectName, 'OnPause', isHidden ? 1 : 0);
    });

    // Событие закрытия вкладки или обновления страницы
    window.addEventListener('beforeunload', function() {
      console.log('[AppEvents] Before unload');
      SendMessage(gameObjectName, 'OnQuit');
    });
    
    console.log('[AppEvents] All event listeners registered');
  }
});