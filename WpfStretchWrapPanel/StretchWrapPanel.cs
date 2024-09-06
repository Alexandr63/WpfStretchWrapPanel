using System.Windows;
using System.Windows.Controls;

namespace WpfStretchWrapPanel
{
    public class StretchWrapPanel : Panel
    {
        #region Private Fields

        private readonly List<Rect> _childrenLayout = new List<Rect>();

        #endregion

        #region Ctor

        public StretchWrapPanel() : base()
        {
            SizeChanged += PanelSizeChangedEventHandler;
        }

        #endregion

        #region Private Methods

        protected override Size MeasureOverride(Size availableSize)
        {
            foreach (UIElement child in InternalChildren)
            {
                child.Measure(availableSize);
            }

            return CalculateLayout(availableSize);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            for (int i = 0; i < InternalChildren.Count; i++)
            {
                InternalChildren[i].Arrange(_childrenLayout[i]);
            }

            return finalSize;
        }

        private void PanelSizeChangedEventHandler(object sender, SizeChangedEventArgs e)
        {
            InvalidateMeasure();
        }

        /// <summary>
        /// Выполняется расчет расположения элементов.
        /// </summary>
        private Size CalculateLayout(Size availableSize)
        {
            _childrenLayout.Clear();

            double panelHeight = 0d;
            double rowHeight = 0d;
            
            double x = 0d;
            double y = 0d;
            bool newRow = true;

            int childIndex = 0;
            while (true)
            {
                if (InternalChildren.Count == childIndex)
                {
                    break;
                }

                UIElement child = InternalChildren[childIndex];

                if (newRow)
                {
                    _childrenLayout.Add(new Rect(new Point(x, y), child.DesiredSize));
                    
                    x += child.DesiredSize.Width;
                    rowHeight = child.DesiredSize.Height;
                    childIndex++;
                    newRow = false;
                }
                else if (x + child.DesiredSize.Width < availableSize.Width)
                {
                    _childrenLayout.Add(new Rect(new Point(x, y), child.DesiredSize));

                    x += child.DesiredSize.Width;
                    if (rowHeight < child.DesiredSize.Height)
                    {
                        rowHeight = child.DesiredSize.Height;
                    }
                    childIndex++;
                }
                else
                {
                    panelHeight += rowHeight;
                    y += rowHeight;
                    x = 0;
                    rowHeight = 0d;
                    newRow = true;
                }
            }

            panelHeight += rowHeight;

            HorizontalStretchChildItems(availableSize.Width);
            LastRowWithSingleItemAlignRight(availableSize.Width);

            return new Size(availableSize.Width, panelHeight);
        }

        /// <summary>
        /// Распределяем элементы в каждой строке равномерно.
        /// </summary>
        private void HorizontalStretchChildItems(double containerWidth)
        {
            List<Rect> childrenLayout = new List<Rect>(_childrenLayout.Count);
            
            foreach (IGrouping<double, Rect> row in _childrenLayout.GroupBy(x => x.Y))
            {
                if (row.Count() > 1)
                {
                    double space = (containerWidth - row.Sum(x => x.Width)) / (row.Count() - 1);

                    if (space > 0)
                    {
                        bool isFirst = true;
                        int counter = 0;
                        foreach (Rect rect in row)
                        {
                            if (!isFirst)
                            {
                                rect.Offset(space * counter, 0);
                            }
                            childrenLayout.Add(rect);

                            isFirst = false;
                            counter++;
                        }
                    }
                    else
                    {
                        childrenLayout.AddRange(row);
                    }
                }
                else
                {
                    childrenLayout.AddRange(row);
                }
            }

            _childrenLayout.Clear();
            _childrenLayout.AddRange(childrenLayout);
        }

        /// <summary>
        /// Если в последней строке один элемент - выравниваем его по правому краю.
        /// </summary>
        private void LastRowWithSingleItemAlignRight(double containerWidth)
        {
            if (_childrenLayout.Count > 1)
            {
                Rect last = _childrenLayout.Last();
                Rect preLast = _childrenLayout[^2];
                if(Math.Abs(last.Y - preLast.Y) > 0.1d)
                {
                    last.Offset(containerWidth - last.Width, 0);
                }

                _childrenLayout.RemoveAt(_childrenLayout.Count - 1);
                _childrenLayout.Add(last);
            }
        }

        #endregion
    }
}

