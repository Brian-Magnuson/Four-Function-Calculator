using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;

namespace FourFunctionCalculator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// The number upon which all operators act. Is displayed to the text window when
        /// <c>UpdateDisplay</c> is called.
        /// </summary>
        private decimal endNum = 0;
        /// <summary>
        /// The number of digits that can be displayed to the screen before the calculator
        /// overflows. A negative sign counts as a digit, but a decimal point does not.
        /// </summary>
        private const int digitLimit = 12;
        /// <summary>
        /// Field representing the current calculator mode.
        /// </summary>
        private CalcMode calcMode = CalcMode.INPUT;
        /// <summary>
        /// Field representing the next binary operation.
        /// </summary>
        private BinOperator binOperator = BinOperator.NONE;

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Takes the current decimal in <c>endNum</c> and attempts to display it
        /// to the text window, rounding if necessary and switching to ERROR mode
        /// if impossible.
        /// </summary>
        private void UpdateDisplay()
        {
            string output = endNum.ToString();
            int wholeDigits = output.IndexOf('.');
            int decimalDigits = output.Length - wholeDigits - 1;

            // endNum is an integer
            if (wholeDigits == -1)
            {
                wholeDigits = output.Length;
            }
            // endNum is a decimal
            else
            {
                int decimalDigitsAllowed = digitLimit - wholeDigits;
                if (wholeDigits <= digitLimit && decimalDigits > decimalDigitsAllowed)
                {
                    output = Decimal.Round(endNum, decimalDigitsAllowed).ToString();
                }
            }

            // Too many whole digits
            if (wholeDigits > digitLimit)
            {
                output = "Error: Overflow";
                calcMode = CalcMode.ERROR;
            }

            display.Text = output;
        }

        /// <summary>
        /// Called when a digit button is clicked. If in OPERATING mode, switches to
        /// INPUT mode, then adds a digit to the display text.
        /// </summary>
        private void AddDigit_Click(object sender, RoutedEventArgs e)
        {
            string digit = (string)((Button)sender).Content;

            if (calcMode == CalcMode.OPERATING)
            {
                display.Text = "0";
                calcMode = CalcMode.INPUT;
            }
            if (display.Text.Equals("0"))
            {
                display.Text = digit;
            }
            // If the display is not at digitLimit and input is enabled
            else if (display.Text.Length < digitLimit && calcMode == CalcMode.INPUT)
            {
                display.Text += digit;
            }
        }

        /// <summary>
        /// Adds a decimal point to the display text.
        /// </summary>
        private void AddPoint_Click(object sender, RoutedEventArgs e)
        {
            if (calcMode == CalcMode.OPERATING)
            {
                display.Text = "0";
                calcMode = CalcMode.INPUT;
            }
            if (!display.Text.Contains('.') && calcMode == CalcMode.INPUT)
            {
                display.Text += '.';
            }
        }

        /// <summary>
        /// Switches to INPUT mode, then changes display text back to "0" without
        /// chaning the value of <c>endNum</c>
        /// </summary>
        private void ClearEntry_Click(object sender, RoutedEventArgs e)
        {
            if (calcMode != CalcMode.ERROR)
            {
                calcMode = CalcMode.INPUT;
                display.Text = "0";
            }
        }

        /// <summary>
        /// Completely resets the calculator, switching to INPUT mode, changing the
        /// display text back to "0", and chancing <c>endNum</c> to 0.
        /// </summary>
        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            calcMode = CalcMode.INPUT;
            display.Text = "0";
            endNum = 0;
        }

        /// <summary>
        /// If the calculator is in INPUT mode, removes the last character entered.
        /// </summary>
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (calcMode == CalcMode.INPUT)
            {
                display.Text = display.Text.Remove(display.Text.Length - 1);
            }
            if (display.Text.Length == 0)
            {
                display.Text = "0";
            }
        }

        /// <summary>
        /// Called when a binary operator button (add, subtract, multiply, divide, equals)
        /// is clicked. Performs the last operation requested, switches to the next operation,
        /// then switches to OPERATING mode.
        /// </summary>
        private void BinOperation_Click(object sender, RoutedEventArgs e)
        {
            // Perform the previous operation
            if (calcMode == CalcMode.INPUT)
            {
                switch (binOperator)
                {
                    case BinOperator.NONE:
                        endNum = Decimal.Parse(display.Text);
                        UpdateDisplay();
                        break;
                    case BinOperator.ADD:
                        endNum += Decimal.Parse(display.Text);
                        UpdateDisplay();
                        break;
                    case BinOperator.SUBTRACT:
                        endNum -= Decimal.Parse(display.Text);
                        UpdateDisplay();
                        break;
                    case BinOperator.MULTIPLY:
                        endNum *= Decimal.Parse(display.Text);
                        UpdateDisplay();
                        break;
                    case BinOperator.DIVIDE:
                        try
                        {
                            endNum /= Decimal.Parse(display.Text);
                            UpdateDisplay();
                        }
                        catch (DivideByZeroException)
                        {
                            endNum = 0;
                            calcMode = CalcMode.ERROR;
                            display.Text = "Error: Undefined";
                        }
                        break;
                }
            }
            
            // Switch to the next operation
            string nextOperation = ((Button)sender).Name;
            binOperator = nextOperation switch
            {
                "add" => BinOperator.ADD,
                "subtract" => BinOperator.SUBTRACT,
                "multiply" => BinOperator.MULTIPLY,
                "divide" => BinOperator.DIVIDE,
                _ => BinOperator.NONE,
            };

            if (calcMode != CalcMode.ERROR)
            {
                calcMode = CalcMode.OPERATING;
            }
        }

        /// <summary>
        /// Called when a unary operator button (negate, sqrt, square, invert, percent)
        /// is clicked. Switches to OPERATING mode, updates <c>endNum</c> immediately,
        /// performs the operation, then updates the display.
        /// </summary>
        private void UniOperation_Click(object sender, RoutedEventArgs e)
        {
            if (calcMode != CalcMode.ERROR)
            {
                calcMode = CalcMode.OPERATING;
                string nextOperation = ((Button)sender).Name;
                endNum = Decimal.Parse(display.Text);
                switch (nextOperation)
                {
                    case "negate":
                        endNum *= -1;
                        UpdateDisplay();
                        break;
                    case "sqrt":
                        try
                        {
                            endNum = (decimal)Math.Sqrt((double)endNum);
                            UpdateDisplay();
                        }
                        catch (OverflowException)
                        {
                            endNum = 0;
                            calcMode = CalcMode.ERROR;
                            display.Text = "Error: NaN";
                        }
                        break;
                    case "square":
                        endNum = (decimal)Math.Pow((double)endNum, 2.0);
                        UpdateDisplay();
                        break;
                    case "invert":
                        try
                        {
                            endNum = 1 / endNum;
                            UpdateDisplay();
                        }
                        catch (DivideByZeroException)
                        {
                            endNum = 0;
                            calcMode = CalcMode.ERROR;
                            display.Text = "Error: Undefined";
                        }
                        break;
                    case "percent":
                        endNum /= 100;
                        UpdateDisplay();
                        break;
                }
                binOperator = BinOperator.NONE;
            }
        }
    }

    /// <summary>
    /// An enumeration for the different calculator modes.
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// INPUT - Allows text to be entered into the display</description>
    /// </item>
    /// <item>
    /// <description>
    /// ERROR - Prevents calculator functions from running; can be removed when
    /// <c>ClearAll_Click</c> is called.</description>
    /// </item>
    /// <item>
    /// <description>
    /// OPERATING - Indicates that an operator button was 
    /// just pressed and the display was just updated.</description>
    /// </item>
    /// </list>
    /// </summary>
    enum CalcMode
    {
        INPUT, ERROR, OPERATING
    }

    /// <summary>
    /// An enumeration for the different binary operators. Members include
    /// NONE, ADD, SUBTRACT, MULTIPLY, and DIVIDE.
    /// </summary>
    enum BinOperator
    {
        NONE, ADD, SUBTRACT, MULTIPLY, DIVIDE
    }
}
