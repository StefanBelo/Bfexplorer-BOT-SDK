// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Forms;

namespace JayBeeBot.Models
{
    /// <summary>
    /// MyBindingList
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class MyBindingList<T> : BindingList<T>
    {
        private Control uiContext;

        /// <summary>
        /// MyBindingList
        /// </summary>
        /// <param name="uiContext"></param>
        public MyBindingList(Control uiContext)
        {
            this.uiContext = uiContext;            
        }

        /// <summary>
        /// Attach
        /// </summary>
        /// <param name="data"></param>
        public void Attach(ObservableCollection<T> data)
        {
            foreach (var item in data)
            {
                this.Add(item);
            }

            data.CollectionChanged += OnDataCollectionChanged;
        }

        /// <summary>
        /// Dettach
        /// </summary>
        /// <param name="data"></param>
        public void Dettach(ObservableCollection<T> data)
        {
            data.CollectionChanged -= OnDataCollectionChanged;

            this.Clear();                
        }
    
        /// <summary>
        /// OnDataCollectionChanged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDataCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (uiContext.InvokeRequired)
            {
                uiContext.Invoke(new Action<object, NotifyCollectionChangedEventArgs>(HandleNotifyCollectionChangedEventArgs), new object[] { sender, e });
            }
            else
            {
                HandleNotifyCollectionChangedEventArgs(sender, e);
            }
        }

        /// <summary>
        /// HandleNotifyCollectionChangedEventArgs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleNotifyCollectionChangedEventArgs(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (T item in e.NewItems)
                    {
                        this.Add(item);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (T item in e.OldItems)
                    {
                        this.Remove(item);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    this.Clear();
                    break;
                default:
                    break;
            }
        }
    }
}