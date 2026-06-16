using System.Collections;
using DevKit.UI.MVVM;
using DevKit.Utils;
using TradeArea.Shared;
using UnityEngine.UIElements;

namespace TradeArea.UI.Custom
{
    [UxmlElement]
    public partial class LoadableListViewWidget : ToolkitWidget
    {
        [UxmlAttribute] private string _listViewName = null;
        [UxmlAttribute] private string _noItemsContainerName = "NoItemsContainer";
        [UxmlAttribute] private string _errorContainerName = "ErrorContainer";
        [UxmlAttribute] private string _loadingContainerName = "LoadingContainer";

        public ListView ListView => _listView;

        private ListView _listView;
        private VisualElement _noItemsContainer;
        private VisualElement _errorContainer;
        private VisualElement _loadingContainer;

        protected override void OnInit()
        {
            _listView = Root.Q<ListView>(_listViewName);

            _errorContainer = Root.Q<VisualElement>(_errorContainerName);
            _loadingContainer = Root.Q<VisualElement>(_loadingContainerName);
            _noItemsContainer = Root.Q<VisualElement>(_noItemsContainerName);

            _listView.SetEnabled(false);
            USSUtil.Hide(_loadingContainer);
            USSUtil.Hide(_errorContainer);
            USSUtil.Hide(_noItemsContainer);
        }

        public void OnLoadingStateChanged<T>(LoadingState<T> x) where T : IList
        {
            switch (x.LoadingFlag)
            {
                case LoadingFlag.Null:
                    _listView.SetEnabled(false);
                    USSUtil.Hide(_loadingContainer);
                    USSUtil.Hide(_errorContainer);
                    USSUtil.Hide(_noItemsContainer);
                    break;

                case LoadingFlag.Loading:
                    _listView.SetEnabled(false);
                    USSUtil.Show(_loadingContainer);
                    USSUtil.Hide(_errorContainer);
                    USSUtil.Hide(_noItemsContainer);
                    break;

                case LoadingFlag.Success:

                    _listView.itemsSource = x.Value;
                    _listView.Rebuild();

                    if (x.Value == null || x.Value.Count == 0)
                    {
                        _listView.SetEnabled(false);
                        USSUtil.Show(_noItemsContainer);
                    }
                    else
                    {
                        _listView.SetEnabled(true);
                        USSUtil.Hide(_noItemsContainer);
                    }

                    USSUtil.Hide(_loadingContainer);
                    USSUtil.Hide(_errorContainer);
                    break;

                case LoadingFlag.Fail:
                    _listView.itemsSource = null;
                    _listView.SetEnabled(false);
                    USSUtil.Hide(_loadingContainer);
                    USSUtil.Hide(_noItemsContainer);
                    USSUtil.Show(_errorContainer);
                    break;
            }
        }
    }
}