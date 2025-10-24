using System;
using System.Threading;
using Core.PooledScroll.Events;
using Core.PooledScroll.Pool;
using Cysharp.Threading.Tasks;
using UI.Controls;
using UnityEngine;

namespace Core.PooledScroll.Behaviors
{
    public abstract class ScrollBehaviour : MonoBehaviour, IDisposable
    {
        public abstract event Action ContentCalculated;
        public abstract event Action<ScrollingEventArgs> ScrollStarted;
        public abstract event Action<ScrollingEventArgs> ScrollEnded;
        public abstract event Action<ScrollingEventArgs> ScrollRangeChanged;
        
        public abstract int ActiveObjects { get; }

        public virtual void Initialize(int count, IScrollObjectPool scrollObjectPool, int startIndex = 0){}
        public abstract void Dispose();

        public virtual UniTask Show(CancellationToken cancellationToken) => UniTask.CompletedTask;
        public virtual UniTask ScrollNext(CancellationToken cancellationToken) => UniTask.CompletedTask;
        public virtual UniTask ScrollBack(CancellationToken cancellationToken) => UniTask.CompletedTask;
        public virtual UniTask ScrollTo(int index, bool immediately = false, CancellationToken cancellationToken = default) => UniTask.CompletedTask;
        
        public abstract int CurrentStartIndex { get; }
        public abstract int CurrentEndIndex { get; }
        
        public virtual void Reinitialize(int count) {}
        
        public virtual int GetMaxVisibleObjects() => 0;
        
        public virtual void CalculateMainContent(int maxElementsCount) { }
    }
}