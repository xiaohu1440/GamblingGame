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
        public static BindableProperty<int> ticketReward=new BindableProperty<int>(12);
        public static BindableProperty<int> greedlevelScore=new BindableProperty<int>(30);
        protected override void Init()
        {
            this.RegisterSystem<IPachinsoSystem>(new PachinsoSystem());
            this.RegisterSystem<IRelicSystem>(new RelicSystem());
            this.RegisterSystem<IEnchantmentSystem>(new EnchantmentSystem());
        }
    }
}

