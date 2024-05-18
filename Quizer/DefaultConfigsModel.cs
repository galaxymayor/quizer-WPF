using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quizer
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    public class DefaultConfigModel : ViewModelBase
    {
        private PartConfigNull _config = new();


        public int ShowIndex
        {
            get => _config.markQuestionNumber == false ? 1 : 0;
            set
            {
                _config.markQuestionNumber = value != 1;
                OnPropertyChanged(nameof(ShowIndex));
            }
        }


        public int IndexAlignment
        {
            get => _config.questionNumberAlignment switch {
                '<' => 1,
                '=' => 2,
                '>' => 3,
                _ => 0

            };
            set
            {
                _config.questionNumberAlignment = value switch
                {
                    1 => '<',
                    2 => '=',
                    3 => '>',
                    _ => null
                };
                OnPropertyChanged(nameof(IndexAlignment));
            }
        }

        public string PartType
        {
            get => _config.partType ?? "";
            set
            {
                _config.partType = value.Length > 0 ? value : null;
                OnPropertyChanged(nameof(PartType));
            }
        }

        public string ALength
        {
            get => _config.underscoreLength?.ToString() ?? "";
            set
            {
                if (int.TryParse(value, out int result))
                {
                    _config.underscoreLength = result;
                }
                else
                {
                    _config.underscoreLength = null;
                }
                OnPropertyChanged(nameof(ALength));
            }
        }

        public int Fixed
        {
            get => _config.underscoreLengthFixed switch
            {
                true => 1,
                false => 2,
                _ => 0
            };

            set
            {
                _config.underscoreLengthFixed = value switch
                {
                    1 => true,
                    2 => false,
                    _ => null
                };
                OnPropertyChanged(nameof(Fixed));
            }
        }

        public string QLength
        {
            get => _config.underscoreInSentenceLength?.ToString() ?? "";
            set
            {
                if (int.TryParse(value, out int result))
                {
                    _config.underscoreInSentenceLength = result;
                }
                else
                {
                    _config.underscoreInSentenceLength = null;
                }
                OnPropertyChanged(nameof(QLength));
            }
        }

        public int ProvideCode
        {
            get => _config.provideCode switch
            {
                true => 1,
                false => 2,
                _ => 0
            };

            set
            {
                _config.provideCode = value switch
                {
                    1 => true,
                    2 => false,
                    _ => null
                };
                OnPropertyChanged(nameof(ProvideCode));
            }
        }

        public int Codes
        {
            get => _config.codes?.Length switch
            {
                31 => 1,
                63 => 2,
                26 => 3,
                _ => 0
            };

            set
            {
                _config.codes = value switch
                {
                    1 => Constants.CARD_E,
                    2 => Constants.CARD_F,
                    3 => Constants.AZ,
                    _ => null
                };
                OnPropertyChanged(nameof(Codes));
            }
        }

        public int LowerCase
        {
            get => _config.lowerCase switch
            {
                true => 1,
                false => 2,
                _ => 0
            };

            set
            {
                _config.lowerCase = value switch
                {
                    1 => true,
                    2 => false,
                    _ => null
                };
                OnPropertyChanged(nameof(LowerCase));
            }
        }

        public string Separator
        {
            get => _config.wordListSeparator ?? "";
            set
            {
                _config.wordListSeparator = value.Length > 0 ? value : null;
                OnPropertyChanged(nameof(Separator));
            }
        }


        public PartConfigNull GetConfig()
        {
            return _config;
        }


        public void SetConfig(in PartConfigNull config)
        {
            if(config.markQuestionNumber != _config.markQuestionNumber)
            {
                _config.markQuestionNumber = config.markQuestionNumber;
                OnPropertyChanged(nameof(ShowIndex));
            }
            if (config.questionNumberAlignment != _config.questionNumberAlignment)
            {
                _config.questionNumberAlignment = config.questionNumberAlignment;
                OnPropertyChanged(nameof(IndexAlignment));
            }
            if (config.partType != _config.partType)
            {
                _config.partType = config.partType;
                OnPropertyChanged(nameof(PartType));
            }
            if (config.underscoreLength != _config.underscoreLength)
            {
                _config.underscoreLength = config.underscoreLength;
                OnPropertyChanged(nameof(ALength));
            }
            if (config.underscoreLengthFixed != _config.underscoreLengthFixed)
            {
                _config.underscoreLengthFixed = config.underscoreLengthFixed;
                OnPropertyChanged(nameof(Fixed));
            }
            if (config.underscoreInSentenceLength != _config.underscoreInSentenceLength)
            {
                _config.underscoreInSentenceLength = config.underscoreInSentenceLength;
                OnPropertyChanged(nameof(QLength));
            }
            if (config.provideCode != _config.provideCode)
            {
                _config.provideCode = config.provideCode;
                OnPropertyChanged(nameof(ProvideCode));
            }
            if (config.codes != _config.codes)
            {
                _config.codes = config.codes;
                OnPropertyChanged(nameof(Codes));
            }
            if (config.lowerCase != _config.lowerCase)
            {
                _config.lowerCase = config.lowerCase;
                OnPropertyChanged(nameof(LowerCase));
            }
            if (config.wordListSeparator != _config.wordListSeparator)
            {
                _config.wordListSeparator = config.wordListSeparator;
                OnPropertyChanged(nameof(Separator));
            }
        }
    }
}
