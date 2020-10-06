using Avalonia;
using Avalonia.Collections;
using Avalonia.Data;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing.Text;
using System.Linq;
using System.Text;

namespace Kanji.Interface.Utilities
{
    class ConditionCollection : AvaloniaList<IAvaloniaObject>
    {
        public event EventHandler<AvaloniaPropertyChangedEventArgs> CollectionIsSatisfied;
        public ConditionCollection()
        {
            CollectionChanged += ConditionCollection_CollectionChanged;
        }

        private void ConditionCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            NotifyCollectionChangedAction collectionChange = eventArgs.Action;

            if (collectionChange == NotifyCollectionChangedAction.Reset)
            {
                foreach (var item in this)
                {
                    VerifyType(item);
                    (item as Condition).EqualityChanged += CollectionCheckEquality;
                }
            }
            else if (collectionChange == NotifyCollectionChangedAction.Add || collectionChange == NotifyCollectionChangedAction.Replace)
            {
                var changedItem = (IAvaloniaObject)eventArgs.NewItems[0];
                VerifyType(changedItem);
                (changedItem as Condition).EqualityChanged += CollectionCheckEquality;
            }
            CollectionCheckEquality(sender, null);
        }

        private void CollectionCheckEquality(object? sender, EventArgs args)
        {
            ConditionEqualityChangedEventArgs collectionArgs = new ConditionEqualityChangedEventArgs
            {
                NewValue = this.Cast<Condition>().All(c => c.Compare()),
            };
            if (collectionArgs.NewValue)
            {
                CollectionIsSatisfied(this, args);
            }
        }

        private static void VerifyType(IAvaloniaObject item)
        {
            if (!(item is Condition))
            {
                throw new InvalidOperationException("Only Condition types are supported in a ConditionCollection.");
            }
        }
    }
}
}
