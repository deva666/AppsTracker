using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows;
using System.Collections;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using WPFHelper;

namespace Task_Logger_Pro.Controls
{
    public class FilterableListView : SortableListView
    {
        public static readonly ICommand ShowFilter = new RoutedCommand( );
        ArrayList filterList;

        public object FilterType
        {
            get { return ( object ) GetValue( FilterTypeProperty ); }
            set { SetValue( FilterTypeProperty, value ); }
        }

        public static readonly DependencyProperty FilterTypeProperty =
    DependencyProperty.Register( "FilterType", typeof( object ), typeof( FilterableListView ), new PropertyMetadata( null ) );

        struct FilterStruct
        {
            public Button button;
            public FilterItem value;
            public string property;

            public FilterStruct( String property, Button button, FilterItem value )
            {
                this.value = value;
                this.button = button;
                this.property = property;
            }
        }

        private Hashtable currentFilters = new Hashtable( );

        private void AddFilter( String property, FilterItem value, Button button )
        {
            if ( currentFilters.ContainsKey( property ) )
            {
                currentFilters.Remove( property );
            }
            currentFilters.Add( property, new FilterStruct( property, button, value ) );
        }

        private bool IsPropertyFiltered( String property )
        {
            foreach ( String filterProperty in currentFilters.Keys )
            {
                FilterStruct filter = ( FilterStruct ) currentFilters[filterProperty];
                if ( filter.property == property )
                    return true;
            }

            return false;
        }

        public FilterableListView( )
        {
            CommandBindings.Add( new CommandBinding( ShowFilter, ShowFilterCommand ) );
        }

        private void ShowFilterCommand( object sender, ExecutedRoutedEventArgs e )
        {
            Button button = e.OriginalSource as Button;

            if ( button != null )
            {
                // navigate up to the header
                GridViewColumnHeader header = ( GridViewColumnHeader ) Helpers.FindElementOfTypeUp( button, typeof( GridViewColumnHeader ) );

                // then down to the popup
                Popup popup = ( Popup ) Helpers.FindElementOfType( header, typeof( Popup ) );

                if ( popup != null )
                {
                    // find the property name that we are filtering
                    SortableGridViewColumn column = ( SortableGridViewColumn ) header.Column;
                    String propertyName = column.SortPropertyName;


                    // clear the previous filter
                    if ( filterList == null )
                    {
                        filterList = new ArrayList( );
                    }
                    filterList.Clear( );

                    // if this property is currently being filtered, provide an option to clear the filter.
                    if ( IsPropertyFiltered( propertyName ) )
                    {
                        filterList.Add( new FilterItem( "[clear]" ) );
                    }
                    else
                    {
                        bool containsNull = false;
                        //PropertyDescriptor filterPropDesc = TypeDescriptor.GetProperties(typeof(Logging.ProcessLog.LogData))[propertyName];
                        PropertyDescriptor filterPropDesc = TypeDescriptor.GetProperties( FilterType.GetType( ) )[propertyName];
                        // iterate over all the objects in the list
                        foreach ( Object item in Items )
                        {
                            object value = filterPropDesc.GetValue( item );
                            if ( value != null )
                            {
                                FilterItem filterItem = new FilterItem( value as IComparable );
                                if ( !filterList.Contains( filterItem ) )
                                {
                                    filterList.Add( filterItem );
                                }
                            }
                            else
                            {
                                containsNull = true;
                            }
                        }

                        filterList.Sort( );

                        if ( containsNull )
                        {
                            filterList.Add( new FilterItem( null ) );
                        }
                    }

                    // open the popup to display this list
                    popup.DataContext = filterList;
                    CollectionViewSource.GetDefaultView( filterList ).Refresh( );
                    popup.IsOpen = true;

                    // connect to the selection change event
                    ListView listView = ( ListView ) popup.Child;
                    listView.SelectionChanged += SelectionChangedHandler;
                }
            }
        }
        private void ApplyCurrentFilters( )
        {
            if ( currentFilters.Count == 0 )
            {
                Items.Filter = null;
                return;
            }

            // construct a filter and apply it               
            Items.Filter = delegate( object item )
            {
                // when applying the filter to each item, iterate over all of
                // the current filters
                bool match = true;
                foreach ( FilterStruct filter in currentFilters.Values )
                {
                    // obtain the value for this property on the item under test
                    //PropertyDescriptor filterPropDesc = TypeDescriptor.GetProperties(typeof(Logging.ProcessLog.LogData))[filter.property];
                    PropertyDescriptor filterPropDesc = TypeDescriptor.GetProperties( FilterType.GetType( ) )[filter.property];
                    //object itemValue = filterPropDesc.GetValue((Logging.ProcessLog.LogData)item);
                    object itemValue = filterPropDesc.GetValue( item );
                    if ( itemValue != null )
                    {
                        // check to see if it meets our filter criteria
                        if ( !itemValue.Equals( filter.value.Item ) )
                            match = false;
                    }
                    else
                    {
                        if ( filter.value.Item != null )
                            match = false;
                    }
                }
                return match;
            };
        }

        /// <summary>
        /// Handles the selection change event from the filter popup
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectionChangedHandler( object sender, SelectionChangedEventArgs e )
        {
            // obtain the term to filter for
            ListView filterListView = ( ListView ) sender;
            FilterItem filterItem = ( FilterItem ) filterListView.SelectedItem;

            // navigate up to the header to obtain the filter property name
            GridViewColumnHeader header = ( GridViewColumnHeader ) Helpers.FindElementOfTypeUp( filterListView, typeof( GridViewColumnHeader ) );

            SortableGridViewColumn column = ( SortableGridViewColumn ) header.Column;
            String currentFilterProperty = column.SortPropertyName;

            if ( filterItem == null )
                return;

            // determine whether to clear the filter for this column
            if ( filterItem.ItemView.Equals( "[clear]" ) )
            {
                if ( currentFilters.ContainsKey( currentFilterProperty ) )
                {
                    FilterStruct filter = ( FilterStruct ) currentFilters[currentFilterProperty];
                    //filter.button.ContentTemplate = (DataTemplate)dictionary["filterButtonInactiveTemplate"];
                    //if (FilterButtonInactiveStyle != null)
                    //{
                    //    filter.button.Style = FilterButtonInactiveStyle;
                    //}
                    currentFilters.Remove( currentFilterProperty );
                }

                ApplyCurrentFilters( );
            }
            else
            {
                // find the button and apply the active style
                Button button = ( Button ) Helpers.FindVisualElement( header, "filterButton" );
                //button.ContentTemplate = (DataTemplate)dictionary["filterButtonActiveTemplate"];

                //if (FilterButtonActiveStyle != null)
                //{
                //    button.Style = FilterButtonActiveStyle;
                //}

                AddFilter( currentFilterProperty, filterItem, button );
                ApplyCurrentFilters( );
            }

            // navigate up to the popup and close it
            Popup popup = ( Popup ) Helpers.FindElementOfTypeUp( filterListView, typeof( Popup ) );
            popup.IsOpen = false;
        }

    }

    class FilterItem : IComparable
    {
        /// <summary>
        /// The filter item instance
        /// </summary>
        private Object item;

        public Object Item
        {
            get { return item; }
            set { item = value; }
        }

        /// <summary>
        /// The item viewed in the filter drop down list. Typically this is the same as the item
        /// property, however if item is null, this has the value of "[empty]"
        /// </summary>
        private Object itemView;

        public Object ItemView
        {
            get { return itemView; }
            set { itemView = value; }
        }

        public FilterItem( IComparable item )
        {
            this.item = this.itemView = item;
            if ( item == null )
            {
                itemView = "[empty]";
            }
        }

        public override int GetHashCode( )
        {
            return item != null ? item.GetHashCode( ) : 0;
        }

        public override bool Equals( object obj )
        {
            FilterItem otherItem = obj as FilterItem;
            if ( otherItem != null && this.Item != null )
            {
                if ( otherItem.Item.ToString( ) == this.Item.ToString( ) )
                {
                    return true;
                }
            }
            return false;
        }

        public int CompareTo( object obj )
        {
            FilterItem otherFilterItem = ( FilterItem ) obj;

            if ( this.Item == null && obj == null )
            {
                return 0;
            }
            else if ( otherFilterItem.Item != null && this.Item != null )
            {
                return ( ( IComparable ) item ).CompareTo( ( IComparable ) otherFilterItem.item );
            }
            else
            {
                return -1;
            }
        }
    }
}
