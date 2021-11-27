using System;

namespace ACVillagerHuntBot
{
    public class MsgMgr
    {
        public bool Success { get; set; }
        public bool HasMessage { get; set; }
        private string _message = String.Empty;
        public string Message { 
            get {
                return _message;
            }
            set {
                _message = value;
                if (string.IsNullOrEmpty(_message)) {
                    HasMessage = false;
                }
                else {
                    HasMessage = true;
                }
            }
        }
        public string SecondMessage { get; set; }

        public MsgMgr() {
            Success = false;
            Message = String.Empty;
            SecondMessage = String.Empty;
        }
    }
}
