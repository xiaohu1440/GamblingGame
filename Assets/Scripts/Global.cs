using System.Collections;
using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace Gambling
{
    public class Global : Architecture<Global>
    {
        public static BindableProperty<int> lotteryTicket = new BindableProperty<int>(10);
        public static BindableProperty<int> level = new BindableProperty<int>(1);
        public static BindableProperty<int> levelScore = new BindableProperty<int>(10);
        public static BindableProperty<int> chips = new BindableProperty<int>(5);
        public static BindableProperty<int> currentLevelSpinCount = new BindableProperty<int>(0);
        protected override void Init()
        {
            
        }
    }
}

