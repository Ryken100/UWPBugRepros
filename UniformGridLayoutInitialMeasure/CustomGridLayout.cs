using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace UniformGridLayoutInitialMeasure
{
    public class CustomGridLayout : VirtualizingLayout
    {
        #region Properties
        public double MinRowSpacing
        {
            get { return (double)GetValue(MinRowSpacingProperty); }
            set { SetValue(MinRowSpacingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MinRowSpacing.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinRowSpacingProperty =
            DependencyProperty.Register("MinRowSpacing", typeof(double), typeof(CustomGridLayout), new PropertyMetadata(0, OnPropertyChanged));

        public double MinColumnSpacing
        {
            get { return (double)GetValue(MinColumnSpacingProperty); }
            set { SetValue(MinColumnSpacingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MinColumnSpacing.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinColumnSpacingProperty =
            DependencyProperty.Register("MinColumnSpacing", typeof(double), typeof(CustomGridLayout), new PropertyMetadata(0, OnPropertyChanged));

        public double MinItemWidth
        {
            get { return minItemWidth; }
            set { SetValue(MinItemWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MinItemWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinItemWidthProperty =
            DependencyProperty.Register("MinItemWidth", typeof(double), typeof(CustomGridLayout), new PropertyMetadata(double.NaN, OnPropertyChanged));

        public double MinItemHeight
        {
            get { return minItemHeight; }
            set { SetValue(MinItemHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MinItemHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinItemHeightProperty =
            DependencyProperty.Register("MinItemHeight", typeof(double), typeof(CustomGridLayout), new PropertyMetadata(double.NaN, OnPropertyChanged));

        public int MaxRowsOrColumns
        {
            get { return maxRowsOrColumns; }
            set { SetValue(MaxRowsOrColumnsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxRowsOrColumns.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxRowsOrColumnsProperty =
            DependencyProperty.Register("MaxRowsOrColumns", typeof(int), typeof(CustomGridLayout), new PropertyMetadata(int.MaxValue, OnPropertyChanged));

        public Orientation Orientation
        {
            get { return orientation; }
            set { SetValue(OrientationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Orientation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(CustomGridLayout), new PropertyMetadata(Orientation.Horizontal, OnPropertyChanged));


        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (Object.Equals(e.NewValue, e.OldValue))
                return;
            var layout = d as CustomGridLayout;
            if (e.Property == MinItemWidthProperty)
            {
                layout.minItemWidth = (double)e.NewValue;
                layout.InvalidateMeasure();
            }
            else if (e.Property == MinItemHeightProperty)
            {
                layout.minItemHeight = (double)e.NewValue;
                layout.InvalidateMeasure();
            }
            else if (e.Property == MinRowSpacingProperty)
            {
                layout.minRowSpacing = (double)e.NewValue;
                layout.InvalidateMeasure();
            }
            else if (e.Property == MinColumnSpacingProperty)
            {
                layout.minColumnSpacing = (double)e.NewValue;
                layout.InvalidateMeasure();
            }
            else if (e.Property == MaxRowsOrColumnsProperty)
            {
                layout.maxRowsOrColumns = (int)e.NewValue;
                layout.InvalidateMeasure();
            }
            else if (e.Property == OrientationProperty)
            {
                layout.orientation = (Orientation)e.NewValue;
                layout.InvalidateMeasure();
            }
        }
        #endregion

        double minRowSpacing = 0;
        double minColumnSpacing = 0;
        double minItemWidth = double.NaN;
        double minItemHeight = double.NaN;
        int maxRowsOrColumns = int.MaxValue;
        Orientation orientation = Orientation.Horizontal;

        protected override void InitializeForContextCore(VirtualizingLayoutContext context)
        {
            context.LayoutState = new CustomGridLayoutState(this);
            base.InitializeForContextCore(context);
        }

        protected override void UninitializeForContextCore(VirtualizingLayoutContext context)
        {
            context.LayoutState = null;
            base.UninitializeForContextCore(context);
        }

        protected override Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize)
        {
            if (context.ItemCount > 0)
            {
                var state = context.LayoutState as CustomGridLayoutState;
                var initialMeasureSize = availableSize;
                ref double containerSize = ref Unsafe.Add(ref Unsafe.As<Size, double>(ref initialMeasureSize), orientation == Orientation.Horizontal ? 0 : 1);
                state.CalculateElementSize(availableSize);
                state.PerformMeasurementOfFirstElement(context, availableSize);
                state.MeasureAllElements(context);
                return state.DesiredSize;
            }
            else
            {
                return base.MeasureOverride(context, availableSize);
            }
        }

        protected override Size ArrangeOverride(VirtualizingLayoutContext context, Size finalSize)
        {
            var state = context.LayoutState as CustomGridLayoutState;
            state.ArrangeAllElements(context, finalSize);
            return finalSize;
            return base.ArrangeOverride(context, finalSize);
        }

        class CustomGridLayoutState
        {
            Dictionary<int, UIElement> elementMap = new Dictionary<int, UIElement>();
            CustomGridLayout layout;
            Size intialElementMeasureSize;

            public double ElementMinorSize;
            public double ElementMajorSize;
            public double MinorSpacing;
            public double MajorSpacing;

            public int MinorRowColumnCount;
            public int MajorRowColumnCount;

            public double FullMinorSize;
            public double FullMajorSize;

            public Size DesiredSize;
            public Size MeasureSize;

            int RealizedStartingIndex;
            int RealizedEndingIndex;
            double MajorArrangeStartPosition;

            public CustomGridLayoutState(CustomGridLayout layout)
            {
                this.layout = layout;
            }

            public void CalculateElementSize(Size availableSize)
            {
                var containerMinorSize = layout.orientation == Orientation.Horizontal ? availableSize.Width : availableSize.Height;
                var containerMajorSize = layout.orientation == Orientation.Horizontal ? availableSize.Height : availableSize.Width;

                var itemMinorSize = layout.orientation == Orientation.Horizontal ? layout.minItemWidth : layout.minItemHeight;
                var itemMajorSize = layout.orientation == Orientation.Horizontal ? layout.minItemHeight : layout.minItemWidth;

                var itemMinorSpacing = layout.orientation == Orientation.Horizontal ? layout.minColumnSpacing : layout.minRowSpacing;
                var itemMajorSpacing = layout.orientation == Orientation.Horizontal ? layout.minRowSpacing : layout.minColumnSpacing;

                if (itemMinorSize <= 0 || !isRealSize(itemMinorSize))
                {
                    itemMinorSize = containerMinorSize;
                }

                var division = containerMinorSize / (itemMinorSize + itemMinorSpacing);
                if (division % 1d >= 0.5)
                {
                    division += 1;
                }
                MinorRowColumnCount = (int)Math.Floor(Math.Max(division, 1));
                if (layout.maxRowsOrColumns > 0 && MinorRowColumnCount > layout.maxRowsOrColumns)
                    MinorRowColumnCount = layout.maxRowsOrColumns;

                ElementMinorSize = (containerMinorSize - (itemMinorSpacing * (MinorRowColumnCount - 1))) / MinorRowColumnCount;

                if (isRealSize(itemMajorSize))
                {
                    ElementMajorSize = itemMajorSize;
                }
                else
                {
                    ElementMajorSize = double.NaN;
                }

                MinorSpacing = itemMinorSpacing;
                MajorSpacing = itemMajorSpacing;
            }

            public void PerformMeasurementOfFirstElement(VirtualizingLayoutContext context, Size availableSize)
            {
                if (context.ItemCount > 0)
                {
                    if (!isRealSize(ElementMajorSize))
                    {
                        // Measure first element to determine ElementMajorSize
                        var initialMeasureSize = availableSize;
                        if (layout.orientation == Orientation.Horizontal)
                            initialMeasureSize.Width = ElementMinorSize;
                        else initialMeasureSize.Height = ElementMinorSize;

                        var firstElement = context.GetOrCreateElementAt(0);
                        firstElement.Measure(initialMeasureSize);

                        ElementMajorSize = (layout.Orientation == Orientation.Horizontal) ? firstElement.DesiredSize.Height : firstElement.DesiredSize.Width;

                    }
                    MajorRowColumnCount = (int)Math.Floor((double)context.ItemCount / MinorRowColumnCount);
                    if (context.ItemCount % MinorRowColumnCount > 0)
                        MajorRowColumnCount++;
                }
                else
                {
                    MajorRowColumnCount = 0;
                }
                FullMinorSize = (layout.orientation == Orientation.Horizontal) ? availableSize.Width : availableSize.Height;
                FullMajorSize = (ElementMajorSize * MajorRowColumnCount) + (MajorSpacing * (MajorRowColumnCount - 1));

                if (layout.orientation == Orientation.Horizontal)
                    MeasureSize = new Size(ElementMinorSize, ElementMajorSize);
                else MeasureSize = new Size(ElementMajorSize, ElementMinorSize);

                if (layout.orientation == Orientation.Horizontal)
                    DesiredSize = new Size(FullMinorSize, FullMajorSize);
                else DesiredSize = new Size(FullMajorSize, FullMinorSize);
            }

            public void MeasureAllElements(VirtualizingLayoutContext context)
            {
                var bounds = context.RealizationRect;
                var boundsStart = layout.orientation == Orientation.Horizontal ? bounds.Top : bounds.Left;
                var boundsEnd = layout.orientation == Orientation.Horizontal ? bounds.Bottom : bounds.Right;

                var count = context.ItemCount;

                int majorRowColumnIndex = GetMajorRowColumnIndexFromPosition(boundsStart);

                var oldStartingIndex = RealizedStartingIndex;
                var oldEndingIndex = RealizedEndingIndex;

                RealizedStartingIndex = GetStartingItemIndexForMajorRowColumnIndex(majorRowColumnIndex);
                RealizedEndingIndex = Math.Min(count, GetEndingItemIndexFromMajorPosition(boundsEnd) + 1);

                void recycle(int range1, int range2)
                {
                    if (range1 >= range2)
                        return;
                    for (int i = range1; i < range2; i++)
                    {
                        RecycleElementAt(i, context);
                    }
                }

                if (oldStartingIndex != RealizedStartingIndex || oldEndingIndex != RealizedEndingIndex)
                {
                    // The old and new realization ranges intersect, so recycle the elements outside of their intersection range
                    if ((oldStartingIndex <= RealizedStartingIndex && oldEndingIndex >= RealizedEndingIndex) ||
                        (oldStartingIndex >= RealizedStartingIndex && oldStartingIndex <= RealizedEndingIndex) ||
                        (oldEndingIndex >= RealizedStartingIndex && oldEndingIndex <= RealizedEndingIndex)
                        )
                    {
                        recycle(oldStartingIndex, RealizedStartingIndex);
                        recycle(RealizedEndingIndex, oldEndingIndex);
                    }
                    // The old and new realization ranges do not interset, so recycle every element in the old range 
                    else
                    {
                        recycle(oldStartingIndex, oldEndingIndex);
                    }
                }

                MajorArrangeStartPosition = GetMajorRowColumnPositionFromRowColumnIndex(majorRowColumnIndex);

                for (int i = RealizedStartingIndex; i < RealizedEndingIndex; i++)
                {
                    var child = GetOrCreateElementAt(i, context);
                    if (child != null)
                        child.Measure(MeasureSize);
                }
            }

            public unsafe void ArrangeAllElements(VirtualizingLayoutContext context, Size finalSize)
            {
                float elementMinorSize = (float)ElementMinorSize;
                float elementMajorSize = (float)ElementMajorSize;
                float minorSpacing = (float)MinorSpacing;
                float majorSpacing = (float)MajorSpacing;

                float minorPosAddition = elementMinorSize + minorSpacing;
                float majorPosAddition = elementMajorSize + majorSpacing;

                float* rectMemory = stackalloc float[4];

                ref float minorPos = ref rectMemory[layout.orientation == Orientation.Horizontal ? 0 : 1];
                ref float majorPos = ref rectMemory[layout.orientation == Orientation.Horizontal ? 1 : 0];

                rectMemory[layout.orientation == Orientation.Horizontal ? 2 : 3] = elementMinorSize;
                rectMemory[layout.orientation == Orientation.Horizontal ? 3 : 2] = elementMajorSize;

                majorPos = (float)MajorArrangeStartPosition;


                ref Rect rect = ref *(Rect*)rectMemory;

                for (int i = RealizedStartingIndex; i < RealizedEndingIndex; i++)
                {
                    var child = GetElementAt(i);
                    if (child != null)
                    {
                        child.Arrange(rect);
                        minorPos += minorPosAddition;
                        if (minorPos >= FullMinorSize)
                        {
                            minorPos = 0;
                            majorPos += majorPosAddition;
                        }
                    }
                }
            }

            public int GetMajorRowColumnIndexFromPosition(double position)
            {
                if (ElementMajorSize <= 0)
                    return 0;
                var index = position / (ElementMajorSize + MajorSpacing);
                return (int)Math.Min(Math.Max(Math.Floor(index), 0), MajorRowColumnCount);
            }

            public int GetStartingItemIndexForMajorRowColumnIndex(int majorRowColumnIndex)
            {
                return MinorRowColumnCount * majorRowColumnIndex;
            }
            public int GetEndingItemIndexForMajorRowColumnIndex(int majorRowColumnIndex)
            {
                return (MinorRowColumnCount * majorRowColumnIndex) + (MinorRowColumnCount - 1);
            }

            public int GetStartingItemIndexFromMajorPosition(double majorPosition)
            {
                if (ElementMajorSize <= 0)
                    return 0;
                return GetStartingItemIndexForMajorRowColumnIndex(GetMajorRowColumnIndexFromPosition(majorPosition));
            }
            public int GetEndingItemIndexFromMajorPosition(double majorPosition)
            {
                if (ElementMajorSize <= 0)
                    return 0;
                return GetEndingItemIndexForMajorRowColumnIndex(GetMajorRowColumnIndexFromPosition(majorPosition));
            }

            public double GetMajorRowColumnPositionFromRowColumnIndex(double majorRowColumnIndex)
            {
                if (ElementMajorSize <= 0)
                    return 0;
                return majorRowColumnIndex * (ElementMajorSize + MajorSpacing);
            }
            UIElement GetOrCreateElementAt(int index, VirtualizingLayoutContext context)
            {
                if (!elementMap.TryGetValue(index, out var element))
                {
                    element = context.GetOrCreateElementAt(index, ElementRealizationOptions.SuppressAutoRecycle);
                    elementMap.Add(index, element);
                }
                return element;
            }
            UIElement GetElementAt(int index)
            {
                elementMap.TryGetValue(index, out var element);
                return element;
            }

            void RecycleElementAt(int index, VirtualizingLayoutContext context)
            {
                if (elementMap.TryGetValue(index, out var element))
                {
                    context.RecycleElement(element);
                    elementMap.Remove(index);
                }
            }

            bool isRealSize(double value)
            {
                return value > 0 && !double.IsInfinity(value) && !double.IsNaN(value);
            }
        }
    }
}
