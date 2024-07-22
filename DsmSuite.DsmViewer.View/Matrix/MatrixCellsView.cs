using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using DsmSuite.DsmViewer.ViewModel.Matrix;

namespace DsmSuite.DsmViewer.View.Matrix
{
    /// <summary>
    /// The view for the square block of cells in a matrix.
    /// </summary>
    public class MatrixCellsView : MatrixFrameworkElement
    {
        private MatrixViewModel _viewModel;
        private readonly MatrixTheme _theme;
        private Rect _rect;     // Area of the cell that is being rendered (reused)
        private int? _hoveredRow;
        private int? _hoveredColumn;
        private readonly double _pitch;     // Distance between the same points in neighbouring cells
        private readonly double _offset;    // Distance between header and first cell (hor/ver)
        private readonly double _verticalTextOffset = 11.0; // Distance between top of cell and baseline of text

        public MatrixCellsView()
        {
            _theme = new MatrixTheme(this);
            _rect = new Rect(new Size(_theme.MatrixCellSize, _theme.MatrixCellSize));
            _hoveredRow = null;
            _hoveredColumn = null;
            _pitch = _theme.MatrixCellSize + _theme.SpacingWidth;
            _offset = _theme.SpacingWidth / 2;

            DataContextChanged += OnDataContextChanged;
            MouseMove += OnMouseMove;
            MouseDown += OnMouseDown;
            MouseLeave += OnMouseLeave;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _viewModel = DataContext as MatrixViewModel;
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged += OnPropertyChanged;
                InvalidateVisual();
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            int row = GetHoveredRow(e.GetPosition(this));
            int column = GetHoveredColumn(e.GetPosition(this));
            if ((_hoveredRow != row) || (_hoveredColumn != column))
            {
                _hoveredRow = row;
                _hoveredColumn = column;
                _viewModel.HoverCell(row, column);
            }
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            _viewModel.HoverCell(null, null);
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            int row = GetHoveredRow(e.GetPosition(this));
            int column = GetHoveredColumn(e.GetPosition(this));
            _viewModel.SelectCell(row, column);
        }

        private void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MatrixViewModel.CellToolTipViewModel))
            {
                ToolTip = _viewModel.CellToolTipViewModel;
            }

            if ((e.PropertyName == nameof(MatrixViewModel.MatrixSize)) ||
                (e.PropertyName == nameof(MatrixViewModel.HoveredRow)) ||
                (e.PropertyName == nameof(MatrixViewModel.SelectedRow)) ||
                (e.PropertyName == nameof(MatrixViewModel.HoveredColumn)) ||
                (e.PropertyName == nameof(MatrixViewModel.SelectedColumn)))
            {
                InvalidateVisual();
            }
        }

        protected override void OnRender(DrawingContext dc)
        {
            if (_viewModel != null)
            {
                SolidColorBrush weightBrush = _theme.CellWeightColor;
                Rect weightRect = new Rect(0, 0, _theme.MatrixCellSize, 0.5 * _theme.MatrixCellSize);

                int matrixSize = _viewModel.MatrixSize;
                for (int row = 0; row < matrixSize; row++)
                {
                    for (int column = 0; column < matrixSize; column++)
                    {
                        _rect.X = _offset + column * _pitch;
                        _rect.Y = _offset + row * _pitch;

                        bool isHovered = (_viewModel.HoveredRow.HasValue && (row == _viewModel.HoveredRow.Value)) ||
                                         (_viewModel.HoveredColumn.HasValue && (column == _viewModel.HoveredColumn.Value));
                        bool isSelected = (_viewModel.SelectedRow.HasValue && (row == _viewModel.SelectedRow.Value)) ||
                                          (_viewModel.SelectedColumn.HasValue && (column == _viewModel.SelectedColumn.Value));
                        MatrixColor color = _viewModel.CellColors[row][column];
                        SolidColorBrush background = _theme.GetBackground(color, isHovered, isSelected);

                        dc.DrawRectangle(background, null, _rect);

                        int weight = _viewModel.CellWeights[row][column];
                        if (weight > 0)
                        {
                            //---- Weight as a filled block
                            weightRect.X = _rect.X;
                            weightRect.Y = _rect.Y + 0.5 * _rect.Height;
                            weightRect.Width = _theme.MatrixCellSize * _viewModel.WeightPercentiles[row][column];
                            dc.DrawRectangle(weightBrush, null, weightRect);

                            //---- Weight as a number
                            char infinity = '\u221E';
                            string content = weight > 9999 ? infinity.ToString() : weight.ToString();

                            double textWidth = MeasureText(content);

                            Point location = new Point
                            {
                                X = (column * _pitch) + (_pitch - textWidth) / 2,
                                Y = (row * _pitch) + _verticalTextOffset
                            };
                            DrawText(dc, content, location, _theme.TextColor, _rect.Width - _theme.SpacingWidth);
                        }
                    }
                }
                Height = Width = _pitch * matrixSize;
            }
        }

        private int GetHoveredRow(Point location)
        {
            double row = (location.Y - _offset) / _pitch;
            return (int)row;
        }

        private int GetHoveredColumn(Point location)
        {
            double column = (location.X - _offset) / _pitch;
            return (int)column;
        }
    }
}
