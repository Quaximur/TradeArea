using System.Threading;
using Cysharp.Threading.Tasks;
using DevKit.Utils;
using R3;
using UnityEngine;
using VContainer;

namespace TradeArea.Infrastructure.Installers
{
    /// <summary>
    /// Загрузчик. Представляет собой базовый класс для регистрации и инициализации механик, 
    /// логически объединённых в один модуль. Нужен для разделения проекта на отдельные модули, 
    /// что позволяет включать/отключать их при необходимости без нарушения работоспособности остальной части проекта, 
    /// переносить их в другие приложения, явно формировать зависимости между механиками и уменьшать "спагетти"-код. 
    /// </summary>
    public abstract class ModuleInstaller : MonoBehaviour
    {
        protected CompositeDisposable _disposables = new();

        protected IObjectResolver Container { get; private set; }

        /// <summary>
        /// Вызов регистраций зависимостей модуля. Используется в <see cref="Bootstrap.InitChildrenAsync">
        /// </summary>
        public async UniTask Initialize(IObjectResolver container, CancellationToken token)
        {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token, destroyCancellationToken);
            var linkedToken = linkedCts.Token;

            linkedToken.ThrowIfCancellationRequested();

            Container = container;

            await OnInitialize(token);
        }

        /// <summary>
        /// Метод для регистрации сущностей в контейнере.
        /// </summary>
        public abstract void Install(IContainerBuilder builder);

        /// <summary>
        /// Метод для получения сущностей из контейнера после его сборки
        /// и инициализации механик данного модуля.
        /// </summary>
        protected abstract UniTask OnInitialize(CancellationToken token);

        protected virtual void OnDestroy()
        {
            Disposes.ClearDispose(ref _disposables);
        }
    }
}