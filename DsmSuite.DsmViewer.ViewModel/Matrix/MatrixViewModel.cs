using System.Collections.ObjectModel;
using DsmSuite.DsmViewer.ViewModel.Common;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using DsmSuite.DsmViewer.Application.Interfaces;
using DsmSuite.DsmViewer.Model.Interfaces;
using DsmSuite.DsmViewer.ViewModel.Main;
using DsmSuite.DsmViewer.ViewModel.Lists;
using System.Security.RightsManagement;

namespace DsmSuite.DsmViewer.ViewModel.Matrix
{
    public class MatrixViewModel : ViewModelBase, IMatrixViewModel
    {
        private double _zoomLevel;
        private readonly IMainViewModel _mainViewModel;
        private readonly IDsmApplication _application;
        private readonly IEnumerable<IDsmElement> _selectedElements;
        private ObservableCollection<ElementTreeItemViewModel> _elementViewModelTree;
        private List<ElementTreeItemViewModel> _elementViewModelLeafs;
        private ElementTreeItemViewModel _selectedTreeItem;
        private ElementTreeItemViewModel _hoveredTreeItem;
        private int? _selectedRow;
        private int? _selectedColumn;
        private int? _hoveredRow;
        private int? _hoveredColumn;
        private int _matrixSize;
        private bool _isMetricsViewExpanded;

        private List<List<MatrixColor>> _cellColors;
        private List<List<int>> _cellWeights;
        private List<MatrixColor> _columnColors;
        private List<int> _columnElementIds;
        private List<string> _metrics;
        private int? _selectedConsumerId;
        private int? _selectedProviderId;

        private const int _nrWeightBuckets = 10; // Number of buckets (quantiles) for grouping cell weights.
        private List<List<double>> _weightPercentiles;  // The weight bucket for every cell as a percentile

        private ElementToolTipViewModel _columnHeaderTooltipViewModel;
        private CellToolTipViewModel _cellTooltipViewModel;

        private readonly Dictionary<MetricType, string> _metricTypeNames;
        private string _selectedMetricTypeName;
        private MetricType _selectedMetricType;
        private string _searchText = "";

        public MatrixViewModel(IMainViewModel mainViewModel, IDsmApplication application, IEnumerable<IDsmElement> selectedElements)
        {
            _mainViewModel = mainViewModel;
            _application = application;
            _selectedElements = selectedElements;

            ToggleElementExpandedCommand = mainViewModel.ToggleElementExpandedCommand;

            SortElementCommand = mainViewModel.SortElementCommand;
            MoveUpElementCommand = mainViewModel.MoveUpElementCommand;
            MoveDownElementCommand = mainViewModel.MoveDownElementCommand;

            ToggleElementBookmarkCommand = mainViewModel.ToggleElementBookmarkCommand;

            AddChildElementCommand = mainViewModel.AddChildElementCommand;
            AddSiblingElementAboveCommand = mainViewModel.AddSiblingElementAboveCommand;
            AddSiblingElementBelowCommand = mainViewModel.AddSiblingElementBelowCommand;
            ModifyElementCommand = mainViewModel.ModifyElementCommand;
            ChangeElementParentCommand = mainViewModel.ChangeElementParentCommand;
            DeleteElementCommand = mainViewModel.DeleteElementCommand;

            CopyElementCommand = mainViewModel.DeleteElementCommand;
            CutElementCommand = mainViewModel.DeleteElementCommand;
            PasteAsChildElementCommand = mainViewModel.DeleteElementCommand;
            PasteAsSiblingElementAboveCommand = mainViewModel.DeleteElementCommand;
            PasteAsSiblingElementBelowCommand = mainViewModel.DeleteElementCommand;

            ShowElementIngoingRelationsCommand = new RelayCommand<object>(ShowElementIngoingRelationsExecute, ShowElementIngoingRelationsCanExecute);
            ShowElementOutgoingRelationCommand = new RelayCommand<object>(ShowElementOutgoingRelationExecute, ShowElementOutgoingRelationCanExecute);
            ShowElementinternalRelationsCommand = new RelayCommand<object>(ShowElementinternalRelationsExecute, ShowElementinternalRelationsCanExecute);

            ShowElementConsumersCommand = new RelayCommand<object>(ShowElementConsumersExecute, ShowConsumersCanExecute);
            ShowElementProvidedInterfacesCommand = new RelayCommand<object>(ShowProvidedInterfacesExecute, ShowElementProvidedInterfacesCanExecute);
            ShowElementRequiredInterfacesCommand = new RelayCommand<object>(ShowElementRequiredInterfacesExecute, ShowElementRequiredInterfacesCanExecute);
            ShowCellDetailMatrixCommand = mainViewModel.ShowCellDetailMatrixCommand;

            ShowCellRelationsCommand = new RelayCommand<object>(ShowCellRelationsExecute, ShowCellRelationsCanExecute);
            ShowCellConsumersCommand = new RelayCommand<object>(ShowCellConsumersExecute, ShowCellConsumersCanExecute);
            ShowCellProvidersCommand = new RelayCommand<object>(ShowCellProvidersExecute, ShowCellProvidersCanExecute);
            ShowElementDetailMatrixCommand = mainViewModel.ShowElementDetailMatrixCommand;
            ShowElementContextMatrixCommand = mainViewModel.ShowElementContextMatrixCommand;

            ToggleMetricsViewExpandedCommand = new RelayCommand<object>(ToggleMetricsViewExpandedExecute, ToggleMetricsViewExpandedCanExecute);

            PreviousMetricCommand = new RelayCommand<object>(PreviousMetricExecute, PreviousMetricCanExecute);
            NextMetricCommand = new RelayCommand<object>(NextMetricExecute, NextMetricCanExecute);

            Reload();

            ZoomLevel = 1.0;

            _metricTypeNames = new Dictionary<MetricType, string>
            {
                [MetricType.NumberOfElements] = "Internal\nElements",
                [MetricType.RelativeSizePercentage] = "Relative\nSize",
                [MetricType.IngoingRelations] = "Ingoing Relations",
                [MetricType.OutgoingRelations] = "Outgoing\nRelations",
                [MetricType.InternalRelations] = "Internal\nRelations",
                [MetricType.ExternalRelations] = "External\nRelations",
                [MetricType.HierarchicalCycles] = "Hierarchical\nCycles",
                [MetricType.SystemCycles] = "System\nCycles",
                [MetricType.Cycles] = "Total\nCycles",
                [MetricType.CycalityPercentage] = "Total\nCycality"
            };

            _selectedMetricType = MetricType.NumberOfElements;
            SelectedMetricTypeName = _metricTypeNames[_selectedMetricType];
        }

        public ICommand ToggleElementExpandedCommand { get; }

        public ICommand SortElementCommand { get; }
        public ICommand MoveUpElementCommand { get; }
        public ICommand MoveDownElementCommand { get; }

        public ICommand ToggleElementBookmarkCommand { get; }

        public ICommand AddChildElementCommand { get; }
        public ICommand AddSiblingElementAboveCommand { get; }
        public ICommand AddSiblingElementBelowCommand { get; }
        public ICommand ModifyElementCommand { get; }
        public ICommand ChangeElementParentCommand { get; }
        public ICommand DeleteElementCommand { get; }

        public ICommand CopyElementCommand { get; }
        public ICommand CutElementCommand { get; }
        public ICommand PasteAsChildElementCommand { get; }
        public ICommand PasteAsSiblingElementAboveCommand { get; }
        public ICommand PasteAsSiblingElementBelowCommand { get; }

        public ICommand ShowElementIngoingRelationsCommand { get; }
        public ICommand ShowElementOutgoingRelationCommand { get; }
        public ICommand ShowElementinternalRelationsCommand { get; }

        public ICommand ShowElementConsumersCommand { get; }
        public ICommand ShowElementProvidedInterfacesCommand { get; }
        public ICommand ShowElementRequiredInterfacesCommand { get; }
        public ICommand ShowElementDetailMatrixCommand { get; }
        public ICommand ShowElementContextMatrixCommand { get; }

        public ICommand ShowCellRelationsCommand { get; }
        public ICommand ShowCellConsumersCommand { get; }
        public ICommand ShowCellProvidersCommand { get; }
        public ICommand ShowCellDetailMatrixCommand { get; }

        public ICommand PreviousMetricCommand { get; }
        public ICommand NextMetricCommand { get; }

        public ICommand ToggleMetricsViewExpandedCommand { get; }

        public string SelectedMetricTypeName
        {
            get { return _selectedMetricTypeName; }
            set
            {
                _selectedMetricTypeName = value;
                _selectedMetricType = _metricTypeNames.FirstOrDefault(x => x.Value == _selectedMetricTypeName).Key;
                Reload();
                OnPropertyChanged();
            }
        }

        public int MatrixSize
        {
            get { return _matrixSize; }
            set { _matrixSize = value; OnPropertyChanged(); }
        }

        public bool IsMetricsViewExpanded
        {
            get { return _isMetricsViewExpanded; }
            set { _isMetricsViewExpanded = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ElementTreeItemViewModel> ElementViewModelTree
        {
            get { return _elementViewModelTree; }
            private set { _elementViewModelTree = value; OnPropertyChanged(); }
        }

        public IReadOnlyList<MatrixColor> ColumnColors => _columnColors;
        public IReadOnlyList<int> ColumnElementIds => _columnElementIds;
        public IReadOnlyList<IList<MatrixColor>> CellColors => _cellColors;
        public IReadOnlyList<IReadOnlyList<int>> CellWeights => _cellWeights;
        /// <summary>
        /// The weight percentile for every cell as a number between 0 and 1.
        /// These are not actual percentiles, but quantiles with <c>_nrWeightBuckets"</c> buckets,
        /// where bucket 0 is reserved for cells with weight 0.
        /// </summary>
        public IReadOnlyList<IReadOnlyList<double>> WeightPercentiles => _weightPercentiles;
        
        public IReadOnlyList<string> Metrics => _metrics;

        public double ZoomLevel
        {
            get { return _zoomLevel; }
            set { _zoomLevel = value; OnPropertyChanged(); }
        }

        public void Reload()
        {
            BackupSelectionBeforeReload();
            ElementViewModelTree = CreateElementViewModelTree();
            _elementViewModelLeafs = FindLeafElementViewModels();
            DefineColumnColors();
            DefineColumnContent();
            DefineCellColors();
            DefineCellContent();
            DefineMetrics();
            MatrixSize = _elementViewModelLeafs.Count;
            RestoreSelectionAfterReload();
        }

        public void SelectTreeItem(ElementTreeItemViewModel selectedTreeItem)
        {
            SelectCell(null, null);
            for (int row = 0; row < _elementViewModelLeafs.Count; row++)
            {
                if (_elementViewModelLeafs[row] == selectedTreeItem)
                {
                    SelectRow(row);
                }
            }
            _selectedTreeItem = selectedTreeItem;
        }

        public ElementTreeItemViewModel SelectedTreeItem
        {
            get
            {
                ElementTreeItemViewModel selectedTreeItem;
                if (SelectedRow.HasValue && (SelectedRow.Value < _elementViewModelLeafs.Count))
                {
                    selectedTreeItem = _elementViewModelLeafs[SelectedRow.Value];
                }
                else
                {
                    selectedTreeItem = _selectedTreeItem;
                }
                return selectedTreeItem;
            }
        }

        public void HoverTreeItem(ElementTreeItemViewModel hoveredTreeItem)
        {
            HoverCell(null, null);
            for (int row = 0; row < _elementViewModelLeafs.Count; row++)
            {
                if (_elementViewModelLeafs[row] == hoveredTreeItem)
                {
                    HoverRow(row);
                }
            }
            _hoveredTreeItem = hoveredTreeItem;
        }

        public ElementTreeItemViewModel HoveredTreeItem
        {
            get
            {
                ElementTreeItemViewModel hoveredTreeItem;
                if (HoveredRow.HasValue && (HoveredRow.Value < _elementViewModelLeafs.Count))
                {
                    hoveredTreeItem = _elementViewModelLeafs[HoveredRow.Value];
                }
                else
                {
                    hoveredTreeItem = _hoveredTreeItem;
                }
                return hoveredTreeItem;
            }
        }

        public void SelectRow(int? row)
        {
            SelectedRow = row;
            SelectedColumn = null;
            UpdateProviderRows();
            UpdateConsumerRows();

            SelectedCellHasRelationCount = 0;
        }

        public void SelectColumn(int? column)
        {
            SelectedRow = null;
            SelectedColumn = column;
            UpdateProviderRows();
            UpdateConsumerRows();

            SelectedCellHasRelationCount = 0;
        }

        public void SelectCell(int? row, int? columnn)
        {
            SelectedRow = row;
            SelectedColumn = columnn;
            UpdateProviderRows();
            UpdateConsumerRows();

            SelectedCellHasRelationCount = _application.GetRelationCount(SelectedConsumer, SelectedProvider);
        }

        public int? SelectedRow
        {
            get { return _selectedRow; }
            private set { _selectedRow = value; OnPropertyChanged(); }
        }

        public int? SelectedColumn
        {
            get { return _selectedColumn; }
            private set { _selectedColumn = value; OnPropertyChanged(); }
        }

        public void HoverRow(int? row)
        {
            HoveredRow = row;
            HoveredColumn = null;
        }

        public void HoverColumn(int? column)
        {
            HoveredRow = null;
            HoveredColumn = column;
            UpdateColumnHeaderTooltip(column);
        }

        public void HoverCell(int? row, int? columnn)
        {
            HoveredRow = row;
            HoveredColumn = columnn;
            UpdateCellTooltip(row, columnn);
        }

        public int? HoveredRow
        {
            get { return _hoveredRow; }
            private set { _hoveredRow = value; OnPropertyChanged(); }
        }

        public int? HoveredColumn
        {
            get { return _hoveredColumn; }
            private set { _hoveredColumn = value; OnPropertyChanged(); }
        }

        public IDsmElement SelectedConsumer
        {
            get
            {
                IDsmElement selectedConsumer = null;
                if (SelectedColumn.HasValue)
                {
                    selectedConsumer = _elementViewModelLeafs[SelectedColumn.Value].Element;
                }
                return selectedConsumer;
            }
        }

        public IDsmElement SelectedProvider => SelectedTreeItem?.Element;

        public int SelectedCellHasRelationCount { get; private set; }

        public ElementToolTipViewModel ColumnHeaderToolTipViewModel
        {
            get { return _columnHeaderTooltipViewModel; }
            set { _columnHeaderTooltipViewModel = value; OnPropertyChanged(); }
        }

        public CellToolTipViewModel CellToolTipViewModel
        {
            get { return _cellTooltipViewModel; }
            set { _cellTooltipViewModel = value; OnPropertyChanged(); }
        }

        public IEnumerable<string> MetricTypes => _metricTypeNames.Values;

        private ObservableCollection<ElementTreeItemViewModel> CreateElementViewModelTree()
        {
            int depth = 0;
            ObservableCollection<ElementTreeItemViewModel> tree = new ObservableCollection<ElementTreeItemViewModel>();
            foreach (IDsmElement element in _selectedElements)
            {
                ElementTreeItemViewModel viewModel = new ElementTreeItemViewModel(_mainViewModel, this, _application, element, depth);
                tree.Add(viewModel);
                AddElementViewModelChildren(viewModel);
            }
            return tree;
        }

        private void AddElementViewModelChildren(ElementTreeItemViewModel viewModel)
        {
            if (viewModel.Element.IsExpanded)
            {
                foreach (IDsmElement child in viewModel.Element.Children)
                {
                    ElementTreeItemViewModel childViewModel = new ElementTreeItemViewModel(_mainViewModel, this, _application, child, viewModel.Depth + 1);
                    viewModel.AddChild(childViewModel);
                    AddElementViewModelChildren(childViewModel);
                }
            }
            else
            {
                viewModel.ClearChildren();
            }
        }

        private List<ElementTreeItemViewModel> FindLeafElementViewModels()
        {
            List<ElementTreeItemViewModel> leafViewModels = new List<ElementTreeItemViewModel>();

            foreach (ElementTreeItemViewModel viewModel in ElementViewModelTree)
            {
                FindLeafElementViewModels(leafViewModels, viewModel);
            }

            return leafViewModels;
        }

        private void FindLeafElementViewModels(List<ElementTreeItemViewModel> leafViewModels, ElementTreeItemViewModel viewModel)
        {
            if (!viewModel.IsExpanded)
            {
                leafViewModels.Add(viewModel);
            }

            foreach (ElementTreeItemViewModel childViewModel in viewModel.Children)
            {
                FindLeafElementViewModels(leafViewModels, childViewModel);
            }
        }

        private void DefineCellColors()
        {
            int matrixSize = _elementViewModelLeafs.Count;

            _cellColors = new List<List<MatrixColor>>();

            // Define background color
            for (int row = 0; row < matrixSize; row++)
            {
                _cellColors.Add(new List<MatrixColor>());
                for (int column = 0; column < matrixSize; column++)
                {
                    _cellColors[row].Add(MatrixColor.Background);
                }
            }

            // Define expanded block color
            for (int row = 0; row < matrixSize; row++)
            {
                ElementTreeItemViewModel viewModel = _elementViewModelLeafs[row];

                Stack<ElementTreeItemViewModel> viewModelHierarchy = new Stack<ElementTreeItemViewModel>();
                ElementTreeItemViewModel child = viewModel;
                ElementTreeItemViewModel parent = viewModel.Parent;
                while ((parent != null) && (parent.Children[0] == child))
                {
                    viewModelHierarchy.Push(parent);
                    child = parent;
                    parent = parent.Parent;
                }

                foreach (ElementTreeItemViewModel currentViewModel in viewModelHierarchy)
                {
                    int leafElements = 0;
                    CountLeafElements(currentViewModel.Element, ref leafElements);

                    if (leafElements > 0 && currentViewModel.Depth > 0)
                    {
                        MatrixColor expandedColor = MatrixColorConverter.GetColor(currentViewModel.Depth);

                        int begin = row;
                        int end = row + leafElements;

                        for (int rowDelta = begin; rowDelta < end; rowDelta++)
                        {
                            for (int columnDelta = begin; columnDelta < end; columnDelta++)
                            {
                                _cellColors[rowDelta][columnDelta] = expandedColor;
                            }
                        }
                    }
                }
            }

            // Define diagonal color
            for (int row = 0; row < matrixSize; row++)
            {
                int depth = _elementViewModelLeafs[row].Depth;
                MatrixColor dialogColor = MatrixColorConverter.GetColor(depth);
                _cellColors[row][row] = dialogColor;
            }

            // Define cycle color
            for (int row = 0; row < matrixSize; row++)
            {
                for (int column = 0; column < matrixSize; column++)
                {
                    IDsmElement consumer = _elementViewModelLeafs[column].Element;
                    IDsmElement provider = _elementViewModelLeafs[row].Element;
                    CycleType cycleType = _application.IsCyclicDependency(consumer, provider);
                    if (cycleType != CycleType.None)
                    {
                        _cellColors[row][column] = MatrixColor.Cycle;
                    }
                }
            }
        }

        private void CountLeafElements(IDsmElement element, ref int count)
        {
            if (!element.IsExpanded)
            {
                count++;
            }
            else
            {
                foreach (IDsmElement child in element.Children)
                {
                    CountLeafElements(child, ref count);
                }
            }
        }

        private void DefineColumnColors()
        {
            _columnColors = new List<MatrixColor>();
            foreach (ElementTreeItemViewModel provider in _elementViewModelLeafs)
            {
                _columnColors.Add(provider.Color);
            }
        }

        private void DefineColumnContent()
        {
            _columnElementIds = new List<int>();
            foreach (ElementTreeItemViewModel provider in _elementViewModelLeafs)
            {
                _columnElementIds.Add(provider.Element.Order);
            }
        }

        /// <summary>
        /// For every cell, set its weight and its weight bucket.
        /// </summary>
        private void DefineCellContent()
        {
            List<int> sortedWeights = new List<int>();
            List<int> buckets = new List<int>(_nrWeightBuckets);

            int matrixSize = _elementViewModelLeafs.Count;

            //---- Set weight for every cell
            _cellWeights = new List<List<int>>();
            for (int row = 0; row < matrixSize; row++)
            {
                _cellWeights.Add(new List<int>());
                for (int column = 0; column < matrixSize; column++)
                {
                    IDsmElement consumer = _elementViewModelLeafs[column].Element;
                    IDsmElement provider = _elementViewModelLeafs[row].Element;
                    int weight = _application.GetDependencyWeight(consumer, provider);
                    _cellWeights[row].Add(weight);
                    if (weight > 0)
                        sortedWeights.Add(weight);
                }
            }

            //---- Set up weight buckets
            buckets.Add(0);
            if (sortedWeights.Count > 0)
            {
                sortedWeights.Sort();
                int stepSize = sortedWeights.Count / _nrWeightBuckets;
                for (int i = 1; i < _nrWeightBuckets; i++)
                {
                    buckets.Add(sortedWeights[i * stepSize]);
                }
            }

            //---- Assign every cell its weight percentile
            _weightPercentiles = new List<List<double>>();
            for (int row = 0; row < matrixSize; row++)
            {
                _weightPercentiles.Add(new List<double>());
                for (int column = 0; column < matrixSize; column++)
                {
                    int i = buckets.Count-1;
                    while (_cellWeights[row][column] < buckets[i])
                        i--;
                    if (i == 0)     // Bucket 0 is for weight 0 exclusively
                        i = 1;
                    _weightPercentiles[row].Add(i / (double) _nrWeightBuckets);
                }
            }
        }


        private void DefineMetrics()
        {
            _metrics = new List<string>();
            switch (_selectedMetricType)
            {
                case MetricType.NumberOfElements:
                    foreach (ElementTreeItemViewModel viewModel in _elementViewModelLeafs)
                    {
                        int childElementCount = _application.GetElementSize(viewModel.Element);
                        _metrics.Add($"{childElementCount}");
                    }
                    break;
                case MetricType.RelativeSizePercentage:
                    foreach (ElementTreeItemViewModel viewModel in _elementViewModelLeafs)
                    {
                        int childElementCount = _application.GetElementSize(viewModel.Element);
                        int totalElementCount = _application.GetElementCount();
                        double metricCount = (totalElementCount > 0) ? childElementCount * 100.0 / totalElementCount : 0;
                        _metrics.Add($"{metricCount:0.000} %");
                    }
                    break;
                case MetricType.IngoingRelations:
                    foreach (ElementTreeItemViewModel viewModel in _elementViewModelLeafs)
                    {
                        int metricCount = _application.FindIngoingRelations(viewModel.Element).Count();
                        _metrics.Add($"{metricCount}");
                    }
                    break;
                case MetricType.OutgoingRelations:
                    foreach (ElementTreeItemViewModel viewModel in _elementViewModelLeafs)
                    {
                        int metricCount = _application.FindOutgoingRelations(viewModel.Element).Count();
                        _metrics.Add($"{metricCount}");
                    }
                    break;
                case MetricType.InternalRelations:
                    foreach (ElementTreeItemViewModel viewModel in _elementViewModelLeafs)
                    {
                        int metricCount = _application.FindInternalRelations(viewModel.Element).Count();
                        _metrics.Add($"{metricCount}");
                    }
                    break;
                case MetricType.ExternalRelations:
                    foreach (ElementTreeItemViewModel viewModel in _elementViewModelLeafs)
                    {
                        int metricCount = _application.FindExternalRelations(viewModel.Element).Count();
                        _metrics.Add($"{metricCount}");
                    }
                    break;
                case MetricType.HierarchicalCycles:
                    foreach (ElementTreeItemViewModel viewModel in _elementViewModelLeafs)
                    {
                        int metricCount = _application.GetHierarchicalCycleCount(viewModel.Element);
                        _metrics.Add(metricCount > 0 ? $"{metricCount}" : "-");
                    }
                    break;
                case MetricType.SystemCycles:
                    foreach (ElementTreeItemViewModel viewModel in _elementViewModelLeafs)
                    {
                        int metricCount = _application.GetSystemCycleCount(viewModel.Element);
                        _metrics.Add(metricCount > 0 ? $"{metricCount}" : "-");
                    }
                    break;
                case MetricType.Cycles:
                    foreach (ElementTreeItemViewModel viewModel in _elementViewModelLeafs)
                    {
                        int metricCount = _application.GetHierarchicalCycleCount(viewModel.Element) +
                                          _application.GetSystemCycleCount(viewModel.Element);
                        _metrics.Add(metricCount > 0 ? $"{metricCount}" : "-");
                    }
                    break;
                case MetricType.CycalityPercentage:
                    foreach (ElementTreeItemViewModel viewModel in _elementViewModelLeafs)
                    {
                        int cycleCount = _application.GetHierarchicalCycleCount(viewModel.Element) +
                                          _application.GetSystemCycleCount(viewModel.Element);
                        int relationCount = _application.FindInternalRelations(viewModel.Element).Count();
                        double metricCount = (relationCount > 0) ? (cycleCount * 100.0 / relationCount) : 0;
                        _metrics.Add(metricCount > 0 ? $"{metricCount:0.000} %" : "-");
                    }
                    break;
                default:
                    foreach (ElementTreeItemViewModel viewModel in _elementViewModelLeafs)
                    {
                        _metrics.Add("");
                    }
                    break;
            }
        }

        private void ShowCellConsumersExecute(object parameter)
        {
            _mainViewModel.NotifyElementsReportReady(ElementListViewModelType.RelationConsumers, SelectedConsumer, SelectedProvider);
        }

        private bool ShowCellConsumersCanExecute(object parameter)
        {
            return true;
        }

        private void ShowCellProvidersExecute(object parameter)
        {
            _mainViewModel.NotifyElementsReportReady(ElementListViewModelType.RelationProviders, SelectedConsumer, SelectedProvider);
        }

        private bool ShowCellProvidersCanExecute(object parameter)
        {
            return true;
        }

        private void ShowElementIngoingRelationsExecute(object parameter)
        {
            _mainViewModel.NotifyRelationsReportReady(RelationsListViewModelType.ElementIngoingRelations, null, SelectedProvider);
        }

        private bool ShowElementIngoingRelationsCanExecute(object parameter)
        {
            return true;
        }

        private void ShowElementOutgoingRelationExecute(object parameter)
        {
            var relations = _application.FindOutgoingRelations(SelectedProvider);
            _mainViewModel.NotifyRelationsReportReady(RelationsListViewModelType.ElementOutgoingRelations, null, SelectedProvider);
        }

        private bool ShowElementOutgoingRelationCanExecute(object parameter)
        {
            return true;
        }

        private void ShowElementinternalRelationsExecute(object parameter)
        {
            _mainViewModel.NotifyRelationsReportReady(RelationsListViewModelType.ElementInternalRelations, null, SelectedProvider);
        }

        private bool ShowElementinternalRelationsCanExecute(object parameter)
        {
            return true;
        }

        private void ShowElementConsumersExecute(object parameter)
        {
            _mainViewModel.NotifyElementsReportReady(ElementListViewModelType.ElementConsumers, null, SelectedProvider);
        }

        private bool ShowConsumersCanExecute(object parameter)
        {
            return true;
        }

        private void ShowProvidedInterfacesExecute(object parameter)
        {
            _mainViewModel.NotifyElementsReportReady(ElementListViewModelType.ElementProvidedInterface, null, SelectedProvider);
        }

        private bool ShowElementProvidedInterfacesCanExecute(object parameter)
        {
            return true;
        }

        private void ShowElementRequiredInterfacesExecute(object parameter)
        {
            _mainViewModel.NotifyElementsReportReady(ElementListViewModelType.ElementRequiredInterface, null, SelectedProvider);
        }

        private bool ShowElementRequiredInterfacesCanExecute(object parameter)
        {
            return true;
        }

        private void ShowCellRelationsExecute(object parameter)
        {
            _mainViewModel.NotifyRelationsReportReady(RelationsListViewModelType.ConsumerProviderRelations, SelectedConsumer, SelectedProvider);
        }

        private bool ShowCellRelationsCanExecute(object parameter)
        {
            return true;
        }

        private void ToggleMetricsViewExpandedExecute(object parameter)
        {
            IsMetricsViewExpanded = !IsMetricsViewExpanded;
        }

        private bool ToggleMetricsViewExpandedCanExecute(object parameter)
        {
            return true;
        }

        private void PreviousMetricExecute(object parameter)
        {
            _selectedMetricType--;
            SelectedMetricTypeName = _metricTypeNames[_selectedMetricType];
        }

        private bool PreviousMetricCanExecute(object parameter)
        {
            return _selectedMetricType != MetricType.NumberOfElements;
        }

        private void NextMetricExecute(object parameter)
        {
            _selectedMetricType++;
            SelectedMetricTypeName = _metricTypeNames[_selectedMetricType];
        }

        private bool NextMetricCanExecute(object parameter)
        {
            return _selectedMetricType != MetricType.CycalityPercentage;
        }

        private void UpdateColumnHeaderTooltip(int? column)
        {
            if (column.HasValue)
            {
                IDsmElement element = _elementViewModelLeafs[column.Value].Element;
                if (element != null)
                {
                    ColumnHeaderToolTipViewModel = new ElementToolTipViewModel(element, _application);
                }
            }
        }

        private void UpdateCellTooltip(int? row, int? column)
        {
            if (row.HasValue && column.HasValue)
            {
                IDsmElement consumer = _elementViewModelLeafs[column.Value].Element;
                IDsmElement provider = _elementViewModelLeafs[row.Value].Element;

                if ((consumer != null) && (provider != null))
                {
                    int weight = _application.GetDependencyWeight(consumer, provider);
                    CycleType cycleType = _application.IsCyclicDependency(consumer, provider);
                    CellToolTipViewModel = new CellToolTipViewModel(consumer, provider, weight, cycleType);
                }
            }
        }

        private void SelectElement(IDsmElement element)
        {
            SelectElement(ElementViewModelTree, element);
        }

        private void SelectElement(IEnumerable<ElementTreeItemViewModel> tree, IDsmElement element)
        {
            foreach (ElementTreeItemViewModel treeItem in tree)
            {
                if (treeItem.Id == element.Id)
                {
                    SelectTreeItem(treeItem);
                }
                else
                {
                    SelectElement(treeItem.Children, element);
                }
            }
        }

        private void ExpandElement(IDsmElement element)
        {
            IDsmElement current = element.Parent;
            while (current != null)
            {
                current.IsExpanded = true;
                current = current.Parent;
            }
            Reload();
        }

        private void UpdateProviderRows()
        {
            if (SelectedRow.HasValue)
            {
                for (int row = 0; row < _elementViewModelLeafs.Count; row++)
                {
                    _elementViewModelLeafs[row].IsProvider = _cellWeights[row][SelectedRow.Value] > 0;
                }
            }
            else
            {
                for (int row = 0; row < _elementViewModelLeafs.Count; row++)
                {
                    _elementViewModelLeafs[row].IsProvider = false;
                }
            }
        }

        private void UpdateConsumerRows()
        {
            if (SelectedRow.HasValue)
            {
                for (int row = 0; row < _elementViewModelLeafs.Count; row++)
                {
                    _elementViewModelLeafs[row].IsConsumer = _cellWeights[SelectedRow.Value][row] > 0;
                }
            }
            else
            {
                for (int row = 0; row < _elementViewModelLeafs.Count; row++)
                {
                    _elementViewModelLeafs[row].IsConsumer = false;
                }
            }
        }

        private void BackupSelectionBeforeReload()
        {
            _selectedConsumerId = SelectedConsumer?.Id;
            _selectedProviderId = SelectedProvider?.Id;
        }

        private void RestoreSelectionAfterReload()
        {
            for (int i = 0; i < _elementViewModelLeafs.Count; i++)
            {
                if (_selectedProviderId.HasValue && (_selectedProviderId.Value == _elementViewModelLeafs[i].Id))
                {
                    SelectRow(i);
                }

                if (_selectedConsumerId.HasValue && (_selectedConsumerId.Value == _elementViewModelLeafs[i].Id))
                {
                    SelectColumn(i);
                }
            }
        }
    }
}
