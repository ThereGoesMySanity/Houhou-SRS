using Avalonia;
using Avalonia.Data;
using Avalonia.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Kanji.Interface.Utilities
{
    class MultiDataTriggerBehavior : Trigger
    {
        public static readonly DirectProperty<MultiDataTriggerBehavior, ConditionCollection> ConditionsProperty =
            AvaloniaProperty.RegisterDirect<MultiDataTriggerBehavior, ConditionCollection>(nameof(Conditions), t => t.Conditions);

        private ConditionCollection _conditions;
        public ConditionCollection Conditions
        {
            get
            {
                if (_conditions == null)
                {
                    _conditions = new ConditionCollection();
                    _conditions.CollectionIsSatisfied += OnSatisfied;
                }
                return _conditions;
            }
        }
        static MultiDataTriggerBehavior()
        {
        }
        public MultiDataTriggerBehavior()
        {
        }
        private void OnSatisfied(object? sender, AvaloniaPropertyChangedEventArgs args)
        {
            if (AssociatedObject == null)
            {
                return;
            }

            DataBindingHelper.RefreshDataBindingsOnActions(dataTriggerBehavior.Actions);

            // NOTE: In UWP version binding null check is not present but Avalonia throws exception as Bindings are null when first initialized.
            var binding = (sender as ConditionCollection).All(c => (c as Condition).Binding != null);
            if (binding)
            {
                // Some value has changed--either the binding value, reference value, or the comparison condition. Re-evaluate the equation.
                if (args.NewValue)
                {
                    Interaction.ExecuteActions(AssociatedObject, Actions, args);
                }
            }
        }
    }
}
