using System;
using UnityEngine.Events;

namespace Code.InitDatas
{
    public class ChangeSandButtonInitData
    {
        public UnityAction<ChangeSandButton> OnSelect { get; set; }
        public Action OnClickAction { get; set; }
    }
}