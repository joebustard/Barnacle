// **************************************************************************
// *   Copyright (c) 2024 Joe Bustard <barnacle3d@gmailcom>                  *
// *                                                                         *
// *   This file is part of the Barnacle 3D application.                     *
// *                                                                         *
// *   This application is free software. You can redistribute it and/or     *
// *   modify it under the terms of the GNU Library General Public           *
// *   License as published by the Free Software Foundation. Either          *
// *   version 2 of the License, or (at your option) any later version.      *
// *                                                                         *
// *   This application is distributed in the hope that it will be useful,   *
// *   but WITHOUT ANY WARRANTY. Without even the implied warranty of        *
// *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
// *   GNU Library General Public License for more details.                  *
// *                                                                         *
// *************************************************************************

using Barnacle.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace Barnacle.UserControls
{
    public class SymbolGrid : ItemsControl
    {
        public SymbolGrid()
        {
            FrameworkElementFactory factory = new FrameworkElementFactory(typeof(UniformGrid), "UniformGrid");
            Binding bindingColumns = new Binding("UniformGirdColumns")
            {
                Mode = BindingMode.OneWay,
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, GetType(), 1)
            };
            Binding bindingRows = new Binding("UniformGirdRows")
            {
                Mode = BindingMode.OneWay,
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, GetType(), 1)
            };
            factory.SetBinding(UniformGrid.ColumnsProperty, bindingColumns);
            factory.SetBinding(UniformGrid.RowsProperty, bindingRows);

            ItemsPanel = new ItemsPanelTemplate(factory);
        }

        #region UniformGrid number of Columns and Rows control

        private int UniformGirdColumns
        {
            get { return (int)GetValue(UniformGirdColumnsProperty); }
            set { SetValue(UniformGirdColumnsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UniformGirdColumns.  This enables animation, styling, binding, etc...
        private static readonly DependencyProperty UniformGirdColumnsProperty =
            DependencyProperty.Register("UniformGirdColumns", typeof(int), typeof(SymbolGrid), new PropertyMetadata(4));

        private int UniformGirdRows
        {
            get { return (int)GetValue(UniformGirdRowsProperty); }
            set { SetValue(UniformGirdRowsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UniformGirdRows.  This enables animation, styling, binding, etc...
        private static readonly DependencyProperty UniformGirdRowsProperty =
            DependencyProperty.Register("UniformGirdRows", typeof(int), typeof(SymbolGrid), new PropertyMetadata(4));

        public double MinColumnWidth
        {
            get { return (double)GetValue(MinColumnWidthProperty); }
            set { SetValue(MinColumnWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MinColumnWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinColumnWidthProperty =
            DependencyProperty.Register("MinColumnWidth", typeof(double), typeof(SymbolGrid),
                new PropertyMetadata(0d, OnDependencyPropertyChanged));

        public double MinRowHeight
        {
            get { return (double)GetValue(MinRowHeightProperty); }
            set { SetValue(MinRowHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MinRowHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinRowHeightProperty =
            DependencyProperty.Register("MinRowHeight", typeof(double), typeof(SymbolGrid),
                new PropertyMetadata(0d, OnDependencyPropertyChanged));

        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Orientation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(SymbolGrid),
                new PropertyMetadata(Orientation.Horizontal, OnDependencyPropertyChanged));

        private static void OnDependencyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var itemsControl = (SymbolGrid)d;
            itemsControl.OnSizeChanged();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            OnSizeChanged();
            base.OnRenderSizeChanged(sizeInfo);
        }

        private void OnSizeChanged()
        {
            if (ActualWidth == 0 || ActualHeight == 0) return;
            UniformGirdColumns = (MinColumnWidth != 0) ? Math.Max((int)Math.Floor(ActualWidth / MinColumnWidth), 1) : 1;
            UniformGirdRows = (MinRowHeight != 0) ? Math.Max((int)Math.Floor(ActualHeight / MinRowHeight), 1) : 1;
            OnDataChanged();
        }

        #endregion UniformGrid number of Columns and Rows control

        public IEnumerable MainItemsSource
        {
            get { return (IEnumerable)GetValue(MainItemsSourceProperty); }
            set { SetValue(MainItemsSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MainItemsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MainItemsSourceProperty =
            DependencyProperty.Register("MainItemsSource", typeof(IEnumerable), typeof(SymbolGrid),
                new PropertyMetadata(null, OnMainItemsSourceChanged));

        private static void OnMainItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var itemsControl = (SymbolGrid)d;
            if (e.OldValue is INotifyCollectionChanged)
            {
                ((INotifyCollectionChanged)e.OldValue).CollectionChanged -= itemsControl.UniformGirdItemsControl_CollectionChanged;
            }
            if (e.NewValue is INotifyCollectionChanged)
            {
                ((INotifyCollectionChanged)e.NewValue).CollectionChanged += itemsControl.UniformGirdItemsControl_CollectionChanged;
            }

            itemsControl.OnDataChanged();
        }

        private void UniformGirdItemsControl_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnDataChanged();
        }

        private int _pageIndex;
        private bool _isEnd;
        private RelayCommand _previousPageCommand;
        private RelayCommand _nextPageCommand;

        public void OnDataChanged()
        {
            if (MainItemsSource == null) return;
            //_pageIndex = 0;
            var cells = UniformGirdColumns * UniformGirdRows;
            var enumerable = (MainItemsSource as IEnumerable<object>)?.ToArray();
            var itemsSource = enumerable.Skip(_pageIndex * cells).Take(cells);
            _isEnd = enumerable != null && enumerable.Count() <= cells * (_pageIndex + 1);
            // _previousPageCommand?.CanExecuteChanged;
            // _nextPageCommand?.RaiseCanExecuteChanged();

            if (Orientation == Orientation.Vertical)
            {
                var finished = false;
                var itemsSourceArray = itemsSource.ToArray();
                var reorderedItemsSource = new List<object>();
                for (var i = 0; i < UniformGirdRows; i++)
                {
                    for (var j = 0; j < UniformGirdColumns; j++)
                    {
                        var arrayIndex = i + j * UniformGirdColumns;
                        reorderedItemsSource.Add((arrayIndex < itemsSource.Count()) ? itemsSourceArray[j + i] : null);
                        if (arrayIndex + 1 == itemsSource.Count())
                        {
                            finished = true;
                            break;
                        }
                    }
                    if (finished) break;
                }
                ItemsSource = reorderedItemsSource;
            }
            else
            {
                ItemsSource = itemsSource;
            }
        }

        public ICommand PreviousPageCommand => _previousPageCommand ?? (_previousPageCommand = new RelayCommand(o =>
             {
                 _pageIndex--;
                 OnDataChanged();
             }, o => _pageIndex > 0));

        public ICommand NextPageCommand => _nextPageCommand ?? (_nextPageCommand = new RelayCommand(o =>
             {
                 _pageIndex++;
                 OnDataChanged();
             }, o => !_isEnd));
    }
}