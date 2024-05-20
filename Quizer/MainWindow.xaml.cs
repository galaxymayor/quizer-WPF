using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
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

namespace Quizer
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow: Window
    {
        private readonly SolidColorBrush
            SolidBlue  = new(Color.FromRgb(0x05, 0x89, 0xfc)),
            SolidRed   = new(Color.FromRgb(0xd4, 0x38, 0x24)),
            SolidBlack = new(Color.FromRgb(0x00, 0x00, 0x00));
        private bool inputBoxLock;
        private bool inputBoxLockOnce;
        private bool GUIConfigLock;
        private bool GUIAllDisabled;
        private static readonly char[] specialChars = ['[', '|', ']', '@'];

        private DefaultConfigModel fillDefault, vocDefault, matchDefault;

        public MainWindow()
        {
            GUIConfigLock = true;
            InitializeComponent();
            GUIConfigLock = false;
            inputBoxLock = false;
            WriteEnabledGUI(Constants.FALSE_FILTER);
            GUIAllDisabled = true;
            inputBoxLockOnce = false;

            
            fillDefault = setFillGrid.DataContext as DefaultConfigModel;
            vocDefault = setVocGrid.DataContext as DefaultConfigModel;
            matchDefault = setMatchGrid.DataContext as DefaultConfigModel;


            ReadSettings();

            DataObject.AddPastingHandler(inputBox, inputBox_Pasting);
            DataObject.AddPastingHandler(questionBox, QABox_Pasting);
            DataObject.AddPastingHandler(answersBox, QABox_Pasting);
        }

        private void inputBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            TextRange tr = new(inputBox.Selection.Start, inputBox.Selection.End);
            inputBoxLock = true;
            tr.Text = (string)e.DataObject.GetData(DataFormats.UnicodeText);
            inputBoxLock = false;
            Highlight(tr.Start, tr.End);
            e.CancelCommand();
            inputBoxLockOnce = true;

            inputBox.Selection.Select(tr.End, tr.End);
        }


        private void QABox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            TextRange tr = new(((RichTextBox)sender).Selection.Start,((RichTextBox)sender).Selection.End);
            tr.Text = (string)e.DataObject.GetData(DataFormats.UnicodeText);
            e.CancelCommand();

            ((RichTextBox)sender).Selection.Select(tr.End, tr.End);
        }

        private void ReadSettings()
        {
            textScale.Value = Properties.Settings.Default.Zoom;
            textFont.Text = Properties.Settings.Default.TextFont;
            
            fillDefault.SetConfig(Interface.ReadConfig(Properties.Settings.Default.DefaultConfigFill));
            fillDefault.PartType = "fill";
            vocDefault.SetConfig(Interface.ReadConfig(Properties.Settings.Default.DefaultConfigVoc));
            vocDefault.PartType = "voc";
            matchDefault.SetConfig(Interface.ReadConfig(Properties.Settings.Default.DefaultConfigMatch));
            matchDefault.PartType = "match";
        }


        private void WriteSettings()
        {
            Properties.Settings.Default.Zoom = textScale.Value;
            Properties.Settings.Default.TextFont = textFont.Text;

            Properties.Settings.Default.DefaultConfigFill = Interface.WriteConfig(fillDefault.GetConfig());
            Properties.Settings.Default.DefaultConfigVoc = Interface.WriteConfig(vocDefault.GetConfig());
            Properties.Settings.Default.DefaultConfigMatch = Interface.WriteConfig(matchDefault.GetConfig());

            Properties.Settings.Default.Save();
        }

        private void Highlight(TextPointer start, TextPointer end)
        {
            if (inputBoxLock)
            {
                return;
            }
            try
            {
                inputBoxLock = true;
                TextPointer idx = start;
                TextRange toMark = new(start, end);
                if (toMark.IsEmpty)
                {
                    return;
                }
                toMark.ClearAllProperties();
                while (idx.CompareTo(end) == -1)
                {
                    var next = idx.GetNextInsertionPosition(LogicalDirection.Forward);
                    if (next == null)
                    {
                        return;
                    }
                    toMark.Select(idx, next);
                    if (!toMark.IsEmpty)
                    {
                        switch (toMark.Text[0])
                        {
                            case '[':
                            case '|':
                            case ']':
                                toMark.ApplyPropertyValue(TextElement.ForegroundProperty, SolidBlue);
                                break;
                            case '@':
                                toMark.ApplyPropertyValue(TextElement.ForegroundProperty, SolidRed);
                                break;
                            default:
                                //toMark.ApplyPropertyValue(TextElement.ForegroundProperty, SolidBlack);
                                break;
                        }
                    }
                    idx = next;
                    int seek = idx.GetTextInRun(LogicalDirection.Forward).IndexOfAny(specialChars);
                    if (seek == -1)
                    {
                        idx = idx.GetPositionAtOffset(idx.GetTextRunLength(LogicalDirection.Forward));
                    }
                    else
                    {
                        idx = idx.GetPositionAtOffset(seek);
                    }
                }
            }
            finally
            {
                inputBoxLock = false;
            }

        }


        private void AddBracket(in TextPointer selStart, in TextPointer selEnd)
        {
            TextPointer start, end, next;
            TextRange tr;
            string txt = inputBox.Selection.Text;
            if (txt.Length > 1024)
            {
                start = selStart;
                end = selEnd;

                goto mark;
            }
            int startSpaces = txt.TakeWhile(char.IsWhiteSpace).Count();
            if (startSpaces > 0)
            {
                next = selStart.GetPositionAtOffset(startSpaces);
                do
                {
                    start = next;
                    next = start.GetNextInsertionPosition(LogicalDirection.Forward); // maybe null!
                    tr = new TextRange(start, next);
                } while (tr.IsEmpty || tr.Text[0] == ' ');
            }
            else
            {
                string before = selStart.GetTextInRun(LogicalDirection.Backward);
                int shift = -before.Reverse().TakeWhile(char.IsLetter).Count();
                next = selStart.GetPositionAtOffset(shift);
                do
                {
                    start = next;
                    next = start.GetNextInsertionPosition(LogicalDirection.Backward); // maybe null!
                    if (next == null)
                    {
                        break;
                    }
                    tr = new TextRange(next, start);
                } while (tr.IsEmpty || char.IsLetter(tr.Text[0]));
            }


            int endSpaces = txt.Reverse().TakeWhile(char.IsWhiteSpace).Count();
            if (endSpaces > 0)
            {
                next = selEnd.GetPositionAtOffset(-endSpaces);
                do
                {
                    end = next;
                    next = end.GetNextInsertionPosition(LogicalDirection.Backward); // maybe null!
                    tr = new TextRange(next, end);
                } while (tr.IsEmpty || char.IsWhiteSpace(tr.Text[0]));
            }
            else
            {
                string after = selEnd.GetTextInRun(LogicalDirection.Forward);
                int shift = after.TakeWhile(char.IsLetter).Count();
                next = selEnd.GetPositionAtOffset(shift);
                do
                {
                    end = next;
                    next = end.GetNextInsertionPosition(LogicalDirection.Forward); // maybe null!
                    if (next == null)
                    {
                        break;
                    }
                    tr = new TextRange(end, next);
                } while (tr.IsEmpty || char.IsLetter(tr.Text[0]));
            }

            mark:
            try
            {

                inputBox.BeginChange();
                inputBoxLock = true;

                if(start.CompareTo(end) == 0)
                {
                    return;
                }

                tr = new(start, start);
                tr.Text = "[";
                tr.ApplyPropertyValue(ForegroundProperty, SolidBlue);
                if (start.CompareTo(selStart) == -1 && !new TextRange(start, selStart).IsEmpty && selStart.CompareTo(selEnd) == -1)
                {
                    tr = new(selStart, selStart);
                    tr.Text = "|";
                    tr.ApplyPropertyValue(ForegroundProperty, SolidBlue);
                }
                tr = new(end, end);
                tr.Text = "]";
                tr.ApplyPropertyValue(ForegroundProperty, SolidBlue);
                if (end.CompareTo(selEnd) == 1 && !new TextRange(selEnd, end).IsEmpty && selStart.CompareTo(selEnd) == -1)
                {
                    tr = new(selEnd, selEnd);
                    tr.Text = "|";
                    tr.ApplyPropertyValue(ForegroundProperty, SolidBlue);
                }

            }
            finally
            {
                inputBox.EndChange();
                inputBoxLock = false;
            }

        }

        private void mainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            TextRange trSource = new(inputBox.Document.ContentStart, inputBox.Document.ContentEnd);

            if (trSource.Text.Trim().Length == 0)
            {
                return;
            }
            var result = MessageBox.Show(this, "Are you sure you want to quit?", "Quit?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);

            if (result != MessageBoxResult.Yes)
            {
                e.Cancel = true;
            }
        }

        private void inputBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = false;
            inputBoxLockOnce = e.Key == Key.Enter || (e.Key == Key.Z || e.Key == Key.Y) && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl));
        }

        private void RichTextBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            bool ctrlPressed = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            //var sender_ = (RichTextBox)sender;
            if (!ctrlPressed)
            {
                return;
            }
            if (e.Delta > 0 && textScale.Value < 2)
            {
                textScale.Value += 0.1;

            }
            else if (e.Delta < 0 && textScale.Value > 1)
            {
                textScale.Value -= 0.1;
            }
            e.Handled = true;
        }

        private void inputBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle)
            {
                TextPointer selStart = inputBox.Selection.Start, selEnd = inputBox.Selection.End;
                if (selStart.CompareTo(selEnd) == 0)
                {
                    TextPointer p = inputBox.GetPositionFromPoint(e.GetPosition(inputBox), true);
                    AddBracket(p, p);
                }
                else
                {
                    AddBracket(selStart, selEnd);
                }
            }
        }

        private void inputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Q && (Keyboard.IsKeyDown(Key.LeftCtrl)||Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                AddBracket(inputBox.Selection.Start, inputBox.Selection.End);
            }
        }

        private void AddBracketSelection(object sender, RoutedEventArgs args)
        {
            AddBracket(inputBox.Selection.Start, inputBox.Selection.End);
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled= true;
            foreach(char c in e.Text)
            {
                if (c > '9' || c < '0')
                {
                    return;
                }
            }
            e.Handled = false;
        }


        private void TextBox_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void TextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            e.CancelCommand();
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = e.Key == Key.Space;
        }

        private void inputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (inputBoxLock)
            {
                return;
            }
            if (inputBoxLockOnce)
            {
                inputBoxLockOnce = false;
                return;
            }
            //int l = int.MaxValue, r = int.MinValue;
            foreach(TextChange c in e.Changes)
            {
                if (c.AddedLength <= 0)
                {
                    continue;
                }
                Highlight(inputBox.Document.ContentStart.GetPositionAtOffset(c.Offset), inputBox.Document.ContentStart.GetPositionAtOffset(c.Offset+c.AddedLength) ?? inputBox.Document.ContentEnd);

            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TextRange trSource = new(inputBox.Document.ContentStart, inputBox.Document.ContentEnd);
            TextRange trQuestions = new(questionBox.Document.ContentStart, questionBox.Document.ContentEnd);
            TextRange trAnswers = new(answersBox.Document.ContentStart, answersBox.Document.ContentEnd);
            Interface.Result result = Interface.LinesToResult(trSource.Text.Split("\r\n"));
            trQuestions.Text = string.Join("\n--------\n", result.questionParts);
            trAnswers.Text = "";
            foreach (var anss in result.answersParts)
            {
                int idx = anss.Key - 1;
                foreach (var answer in anss.Value)
                {
                    trAnswers.Text += $"{++idx}.{answer}\n";
                }
            }

            if (result.messages.infos.Count > 0)
            {
                MessageBox.Show(string.Join('\n', result.messages.infos), "Information", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
            if (result.messages.warnings.Count > 0)
            {
                MessageBox.Show(string.Join('\n', result.messages.warnings), "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            if (result.messages.errors.Count > 0)
            {
                MessageBox.Show(string.Join('\n', result.messages.Item3), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void inputBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            var sel = inputBox.Selection;
            if(sel.Start.CompareTo(sel.End) != 0)
            {
                return;
            }
            var lineStart = sel.Start.GetLineStartPosition(0);
            var lineEnd = sel.Start.GetLineStartPosition(1) ?? inputBox.Document.ContentEnd;
            var line = new TextRange(lineStart, lineEnd);
            var text = line.Text.TrimEnd();
            if (text.Length == 0 || text[0] != '@' || text.Length>1 && text[1] == '@')
            {
                if(!GUIAllDisabled)
                {
                    WriteConfigGUI(Constants.NULL_CONFIG);
                    WriteEnabledGUI(Constants.FALSE_FILTER);
                    GUIAllDisabled = true;
                }
                return;
            }
            try
            {
                var config = Interface.ReadConfig(text);
                WriteConfigGUI(config);
            }
            catch (Exception)
            {
                return;
            }
        }


        private void GUIConfigToLine()
        {
            try
            {
                var config = ReadConfigGUI();
                var sel = inputBox.Selection;
                var lineStart = sel.Start.GetLineStartPosition(0);
                var lineEnd = (sel.Start.GetLineStartPosition(1) ?? inputBox.Document.ContentEnd).GetPositionAtOffset(-2);
                var line = new TextRange(lineStart, lineEnd);
                line.Text = Interface.WriteConfig(config);

                WriteEnabledGUI(Parts.GetPartConfigFilter(config.partType));
            }
            catch (Exception)
            {
                return;
            }
        }

        private void ConfigGUIChanged<T>(object sender, T e) where T: RoutedEventArgs
        {
            if (GUIConfigLock)
            {
                return;
            }
            GUIConfigToLine();
        }


        private PartConfigFilter ReadEnabledGUI()
        {
            return new()
            {
                partType = blockType.IsEnabled,
                underscoreLength = answerBlankLength.IsEnabled,
                underscoreInSentenceLength = questionBlankLength.IsEnabled,
                index = startIndex.IsEnabled,
                questionNumberAlignment = indexAlignment.IsEnabled,
                markQuestionNumber = showIndex.IsEnabled,
                underscoreLengthFixed = lengthFixed.IsEnabled,
                provideCode = provideCode.IsEnabled,
                codes = codeType.IsEnabled,
                lowerCase = forceLowerCase.IsEnabled,
                wordListSeparator = wordListSeparator.IsEnabled,
            };
        }

        private void buttonSetDefault_Click(object sender, RoutedEventArgs e)
        {
            var defaultConfig = blockType.SelectedIndex switch
            {
                0 => fillDefault.GetConfig(),
                1 => matchDefault.GetConfig(),
                2 => vocDefault.GetConfig(),
                _ => Constants.NULL_CONFIG
            };
            WriteConfigGUI(defaultConfig);
            GUIConfigToLine();
        }

        private void OtherBoxesFocused(object sender, RoutedEventArgs e)
        {
            WriteEnabledGUI(Constants.FALSE_FILTER);
        }

        private void GUIConfigViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollViewer = (ScrollViewer)sender;
            if (e.Delta < 0)
            {
                scrollViewer.LineRight();
            }
            else
            {
                scrollViewer.LineLeft();
            }
            e.Handled = true;
        }

        private void WriteEnabledGUI(in PartConfigFilter filter)
        {
            //try
            //{
                //GUIConfigLock = true;
            blockType.IsEnabled = filter.partType;
            answerBlankLength.IsEnabled = filter.underscoreLength;
            questionBlankLength.IsEnabled = filter.underscoreInSentenceLength;
            startIndex.IsEnabled = filter.index;
            indexAlignment.IsEnabled = filter.questionNumberAlignment;
            showIndex.IsEnabled = filter.markQuestionNumber;
            lengthFixed.IsEnabled = filter.underscoreLengthFixed;
            provideCode.IsEnabled = filter.provideCode;
            codeType.IsEnabled = filter.codes;
            forceLowerCase.IsEnabled = filter.lowerCase;
            wordListSeparator.IsEnabled = filter.wordListSeparator;

            buttonSetDefault.IsEnabled = filter.partType && blockType.SelectedIndex != -1;
            //}
            //finally
            //{
            //    GUIConfigLock = false;
            //}
        }

        private void mainWindow_Closed(object sender, EventArgs e)
        {
            WriteSettings();
        }

        private PartConfigNull ReadConfigGUI()
        {
            PartConfigNull ans = new()
            {
                partType = blockType.SelectedIndex switch
                {
                    0 => "fill",
                    1 => "match",
                    2 => "voc",
                    _ => null
                },
                provideCode = provideCode.IsChecked,
                codes = codeType.SelectedIndex switch
                {
                    0 => Constants.CARD_E,
                    1 => Constants.CARD_F,
                    2 => Constants.AZ,
                    _ => null
                },
                underscoreLength = int.TryParse(answerBlankLength.Text, out int l) ? l : null,
                lowerCase = forceLowerCase.IsChecked,
                wordListSeparator = wordListSeparator.Text.Length > 0 ? wordListSeparator.Text : null,
                index = int.TryParse(startIndex.Text, out int i) ? --i : null,
                markQuestionNumber = showIndex.IsChecked,
                questionNumberAlignment = indexAlignment.SelectedIndex switch
                {
                    0 => '<',
                    1 => '=',
                    2 => '>',
                    _ => null
                },
                underscoreInSentenceLength = int.TryParse(questionBlankLength.Text, out l) ? l : null,
                underscoreLengthFixed = lengthFixed.IsChecked
            };
            //ans.ApplyFilter(ReadEnabledGUI());
            return ans;
        }


        private void WriteConfigGUI(in PartConfigNull config)
        {
            try
            {
                GUIConfigLock = true;
                blockType.SelectedIndex = config.partType switch {
                    "fill" => 0,
                    "match" or "pair" => 1,
                    "voc" => 2,
                    _ => -1
                };
                provideCode.IsChecked = config.provideCode;
                codeType.SelectedIndex = config.codes?.Length switch
                {
                    31 => 0,
                    63 => 1,
                    26 => 2,
                    _ => -1
                };
                answerBlankLength.Text = config.underscoreLength?.ToString() ?? "";
                forceLowerCase.IsChecked = config.lowerCase;
                wordListSeparator.Text = config.wordListSeparator ?? "";

                startIndex.Text = config.index != null ? $"{config.index+1}" : "";
                showIndex.IsChecked = config.markQuestionNumber;
                indexAlignment.SelectedIndex = config.questionNumberAlignment switch
                {
                    '<' => 0,
                    '=' => 1,
                    '>' => 2,
                    _ => -1
                };
                questionBlankLength.Text = config.underscoreInSentenceLength?.ToString() ?? "";
                lengthFixed.IsChecked = config.underscoreLengthFixed;


                WriteEnabledGUI(Parts.GetPartConfigFilter(config.partType));
                GUIAllDisabled = false;
            }
            finally
            {
                GUIConfigLock = false;
            }
        }

    }

}
