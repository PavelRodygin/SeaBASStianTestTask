namespace CodeBase.Services.EventMediator
{
    //TODO Need abstraction for events to properly separate Core and Implementation
    public class PopupOpenedEvent
    {
        //Represents an event that encapsulates the name of the popup that was opened.
        public string PopupName { get; }

        public PopupOpenedEvent(string popupName) => PopupName = popupName;
    }
}