using Avalonia;
using Avalonia.Xaml.Interactions.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace Kanji.Interface.Utilities
{
    class Condition : AvaloniaObject
    {
        static Condition()
        {
            BindingProperty.Changed.Subscribe(e => OnValueChanged(e.Sender, e));
            ComparisonConditionProperty.Changed.Subscribe(e => OnValueChanged(e.Sender, e));
            ValueProperty.Changed.Subscribe(e => OnValueChanged(e.Sender, e));
        }

        public event EventHandler<ConditionEqualityChangedEventArgs> EqualityChanged;
        /// <summary>
        /// Identifies the <seealso cref="Binding"/> avalonia property.
        /// </summary>
        public static readonly StyledProperty<object> BindingProperty =
            AvaloniaProperty.Register<DataTriggerBehavior, object>(nameof(Binding));

        /// <summary>
        /// Identifies the <seealso cref="ComparisonCondition"/> avalonia property.
        /// </summary>
        public static readonly StyledProperty<ComparisonConditionType> ComparisonConditionProperty =
            AvaloniaProperty.Register<DataTriggerBehavior, ComparisonConditionType>(nameof(ComparisonCondition), ComparisonConditionType.Equal);

        /// <summary>
        /// Identifies the <seealso cref="Value"/> avalonia property.
        /// </summary>
        public static readonly StyledProperty<object> ValueProperty =
            AvaloniaProperty.Register<DataTriggerBehavior, object>(nameof(Value));
        /// <summary>
        /// Gets or sets the bound object that the <see cref="DataTriggerBehavior"/> will listen to. This is a avalonia property.
        /// </summary>
        public object Binding
        {
            get => GetValue(BindingProperty);
            set => SetValue(BindingProperty, value);
        }

        /// <summary>
        /// Gets or sets the type of comparison to be performed between <see cref="DataTriggerBehavior.Binding"/> and <see cref="DataTriggerBehavior.Value"/>. This is a avalonia property.
        /// </summary>
        public ComparisonConditionType ComparisonCondition
        {
            get => GetValue(ComparisonConditionProperty);
            set => SetValue(ComparisonConditionProperty, value);
        }

        /// <summary>
        /// Gets or sets the value to be compared with the value of <see cref="DataTriggerBehavior.Binding"/>. This is a avalonia property.
        /// </summary>
        public object Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }
            
        public bool Compare()
        {
            return Compare(Binding, ComparisonCondition, Value);
        }
        private static bool Compare(object? leftOperand, ComparisonConditionType operatorType, object? rightOperand)
        {
            if (leftOperand != null && rightOperand != null)
            {
                rightOperand = TypeConverterHelper.Convert(rightOperand.ToString(), leftOperand.GetType());
            }

            IComparable? leftComparableOperand = leftOperand as IComparable;
            IComparable? rightComparableOperand = rightOperand as IComparable;
            if ((leftComparableOperand != null) && (rightComparableOperand != null))
            {
                return EvaluateComparable(leftComparableOperand, operatorType, rightComparableOperand);
            }

            switch (operatorType)
            {
                case ComparisonConditionType.Equal:
                    return object.Equals(leftOperand, rightOperand);

                case ComparisonConditionType.NotEqual:
                    return !object.Equals(leftOperand, rightOperand);

                case ComparisonConditionType.LessThan:
                case ComparisonConditionType.LessThanOrEqual:
                case ComparisonConditionType.GreaterThan:
                case ComparisonConditionType.GreaterThanOrEqual:
                    {
                        if (leftComparableOperand == null && rightComparableOperand == null)
                        {
                            throw new ArgumentException(string.Format(
                                CultureInfo.CurrentCulture,
                                "Binding property of type {0} and Value property of type {1} cannot be used with operator {2}.",
                                leftOperand != null ? leftOperand.GetType().Name : "null",
                                rightOperand != null ? rightOperand.GetType().Name : "null",
                                operatorType.ToString()));
                        }
                        else if (leftComparableOperand == null)
                        {
                            throw new ArgumentException(string.Format(
                                CultureInfo.CurrentCulture,
                                "Binding property of type {0} cannot be used with operator {1}.",
                                leftOperand != null ? leftOperand.GetType().Name : "null",
                                operatorType.ToString()));
                        }
                        else
                        {
                            throw new ArgumentException(string.Format(
                                CultureInfo.CurrentCulture,
                                "Value property of type {0} cannot be used with operator {1}.",
                                rightOperand != null ? rightOperand.GetType().Name : "null",
                                operatorType.ToString()));
                        }
                    }
            }

            return false;
        }
        /// <summary>
        /// Evaluates both operands that implement the IComparable interface.
        /// </summary>
        private static bool EvaluateComparable(IComparable leftOperand, ComparisonConditionType operatorType, IComparable rightOperand)
        {
            object? convertedOperand = null;
            try
            {
                convertedOperand = Convert.ChangeType(rightOperand, leftOperand.GetType(), CultureInfo.CurrentCulture);
            }
            catch (FormatException)
            {
                // FormatException: Convert.ChangeType("hello", typeof(double), ...);
            }
            catch (InvalidCastException)
            {
                // InvalidCastException: Convert.ChangeType(4.0d, typeof(Rectangle), ...);
            }

            if (convertedOperand == null)
            {
                return operatorType == ComparisonConditionType.NotEqual;
            }

            int comparison = leftOperand.CompareTo((IComparable)convertedOperand);
            switch (operatorType)
            {
                case ComparisonConditionType.Equal:
                    return comparison == 0;

                case ComparisonConditionType.NotEqual:
                    return comparison != 0;

                case ComparisonConditionType.LessThan:
                    return comparison < 0;

                case ComparisonConditionType.LessThanOrEqual:
                    return comparison <= 0;

                case ComparisonConditionType.GreaterThan:
                    return comparison > 0;

                case ComparisonConditionType.GreaterThanOrEqual:
                    return comparison >= 0;
                default:
                    break;
            }

            return false;
        }

        private static void OnValueChanged(IAvaloniaObject avaloniaObject, AvaloniaPropertyChangedEventArgs args)
        {
            var condition = avaloniaObject as Condition;
            var changedArgs = new ConditionEqualityChangedEventArgs { NewValue = condition.Compare() };
            switch (args.Property.Name)
            {
                case nameof(Binding):
                    changedArgs.OldValue = Compare(args.OldValue, condition.ComparisonCondition, condition.Value);
                    break;
                case nameof(ComparisonCondition):
                    changedArgs.OldValue = Compare(condition.Binding, (ComparisonConditionType)args.OldValue, condition.Value);
                    break;
                case nameof(Value):
                    changedArgs.OldValue = Compare(condition.Binding, condition.ComparisonCondition, args.OldValue);
                    break;
            }
            if (changedArgs.OldValue != changedArgs.NewValue)
            {
                condition.EqualityChanged?.Invoke(condition, changedArgs);
            }
        }
    }
    public class ConditionEqualityChangedEventArgs : EventArgs
    {
        public bool OldValue, NewValue;
    }
}
