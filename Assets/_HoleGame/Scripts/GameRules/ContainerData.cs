namespace HoleBox
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [Serializable]
    public class ContainerData
    {
        //set private when load data from json
        public int id;
        public int number;
        public int fakeNumber;
        public int capacity = 8;

        public void AddFakeNumber()
        {
            fakeNumber++;

            if (fakeNumber > capacity)
            {
                fakeNumber = capacity;
            }
        }
        public void MinusFakeNumber()
        {
            fakeNumber--;
            if (fakeNumber < 0)
            {
                fakeNumber = 0;
            }
        }

        public Action<int, bool>           OnUpdateQuantity    = null;
        public Action<int, UfoTransporter> OnUfoUpdateQuantity = null;
        public Action<int, ContainerData>  OnMinus             = null;
        public Action                      OnFullStack         = null;
        public Action                      OnChangeID          = null;
        public Action                      OnEmptyStack        = null;
        public Action                      OnReset             = null;

        public int  ID             => id;
        public int  Number         => number;
        public int  Capacity       => capacity;
        public int  Remaining      => capacity - number;
        public int  FakeRemaining  => capacity - fakeNumber;
        public bool IsBusy         => fakeNumber != number;
        public bool CanUpdateQueue => Remaining != 0;
        public int  FakeNumber     => fakeNumber;

        public void ChangeID(int newID)
        {
            id = newID;
            OnChangeID?.Invoke();
        }

        public void UpdateNumber(int count, bool useUfo = true)
        {
            var temp  = number + count;
            var delta = count;
            number += count;

            if (temp > capacity)
            {
                temp  = capacity;
                delta = temp - number;
            }

            number = temp;

            OnUpdateQuantity?.Invoke(delta, useUfo);

            if (useUfo)
            {
                if (number == capacity)
                {
                    OnFullStack?.Invoke();
                }
            }
        }
        
        public void UpdateNumberWithUfo(int count, UfoTransporter useUfo)
        {
            var temp  = number + count;
            var delta = count;
            number += count;

            if (temp > capacity)
            {
                temp  = capacity;
                delta = temp - number;
            }

            number = temp;

            OnUfoUpdateQuantity?.Invoke(delta, useUfo);
            
            if (number == capacity)
            {
                OnFullStack?.Invoke();
            }
        }

        public void Minus(ContainerData container, int delta)
        {
            number -= delta;
            OnMinus?.Invoke(delta, container);
            if (number > 0) return;
            number = 0;
            id     = -1;
            OnEmptyStack?.Invoke();
        }

        public ContainerData()
        {
            id       = 1;
            number   = 0;
            capacity = 8;
        }

        public ContainerData(int id, int capacity)
        {
            this.id       = id;
            this.capacity = capacity;
        }

        public ContainerData(ContainerData containerData)
        {
            this.id       = containerData.id;
            this.number   = containerData.number;
            this.capacity = containerData.capacity;
        }
        public void Reset()
        {
            id         = -1;
            number     = 0;
            fakeNumber = 0;
            OnReset?.Invoke();
        }
    }

    [Serializable]
    public class ContainerQueueData
    {
        public List<ContainerData> containerDatas;

        public ContainerQueueData() { containerDatas = new List<ContainerData>(); }

        public ContainerQueueData(StaticContainerConfig config)
        {
            containerDatas = new List<ContainerData>();

            for (int i = 0; i < config.Count; i++)
            {
                containerDatas.Add(new ContainerData(-1, config.Capacity));
            }
        }

        public ContainerQueueData(int count, int capacity = 8)
        {
            containerDatas = new List<ContainerData>();

            for (int i = 0; i < count; i++)
            {
                containerDatas.Add(new ContainerData(1, capacity));
            }
        }

        public void AddContainer(ContainerData containerData)
        {
            if (containerData == null) return;
            if (!containerDatas.Contains(containerData))
                containerDatas.Add(containerData);
        }

        public void RemoveContainer(ContainerData containerData)
        {
            if (containerData == null) return;
            containerDatas.Remove(containerData);
        }

        public void AddSlot() { containerDatas.Add(new ContainerData(-1, 8)); }
    }

    [Serializable]
    public class StaticContainerConfig
    {
        public int Count    = 4;
        public int Capacity = 8;
    }
}