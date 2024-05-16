/* This file provides constant values */

namespace quizer_WPF
{
    public readonly struct Constants
    {
        public static readonly string[] CARD_E = [
            "A", "B", "C", "D", "E",
            "AB", "AC", "AD", "AE", "BC", "BD", "BE", "CD", "CE", "DE",
            "ABC", "ABD", "ABE", "ACD", "ACE", "ADE", "BCD", "BCE", "BDE", "CDE",
            "ABCD", "ABCE", "ABDE", "ACDE", "BCDE",
            "ABCDE"
        ],
        CARD_F = [
            "A", "B", "C", "D", "E", "F",
            "AB", "AC", "AD", "AE", "AF", "BC", "BD", "BE", "BF", "CD", "CE", "CF", "DE", "DF", "EF",
            "ABC", "ABD", "ABE", "ABF", "ACD", "ACE", "ACF", "ADE", "ADF", "AEF",
            "BCD", "BCE", "BCF", "BDE", "BDF", "BEF", "CDE", "CDF", "CEF", "DEF",
            "ABCD", "ABCE", "ABCF", "ABDE", "ABDF", "ABEF", "ACDE", "ACDF", "ACEF", "ADEF",
            "BCDE", "BCDF", "BCEF", "BDEF", "CDEF",
            "ABCDE", "ABCDF", "ABCEF", "ABDEF", "ACDEF", "BCDEF",
            "ABCDEF"
        ],
        AZ = [
            "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M",
            "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"
        ];

        public static readonly PartConfigFilter FILL_FILTER = new(
            partType: true,
            underscoreLength: true,
            underscoreInSentenceLength: false,
            index: true,
            questionNumberAlignment: true,
            markQuestionNumber: true,
            underscoreLengthFixed: true,
            provideCode: false,
            lowerCase: false,
            wordListSeparator: false,
            codes: false
        ), VOC_FILTER = new(
            partType: true,
            underscoreLength: true,
            underscoreInSentenceLength: true,
            index: true,
            questionNumberAlignment: false,
            markQuestionNumber: true,
            underscoreLengthFixed: true,
            provideCode: false,
            lowerCase: false,
            wordListSeparator: false,
            codes: false
        ), MATCH_FILTER = new(
            partType: true,
            underscoreLength: true,
            underscoreInSentenceLength: false,
            index: true,
            questionNumberAlignment: true,
            markQuestionNumber: true,
            underscoreLengthFixed: true,
            provideCode: true,
            lowerCase: true,
            wordListSeparator: true,
            codes: true
        ), TRUE_FILTER = new(
            partType: true,
            underscoreLength: true,
            underscoreInSentenceLength: true,
            index: true,
            questionNumberAlignment: true,
            markQuestionNumber: true,
            underscoreLengthFixed: true,
            provideCode: true,
            lowerCase: true,
            wordListSeparator: true,
            codes: true
        ), FALSE_FILTER = new(
            partType: false,
            underscoreLength: false,
            underscoreInSentenceLength: false,
            index: false,
            questionNumberAlignment: false,
            markQuestionNumber: false,
            underscoreLengthFixed: false,
            provideCode: false,
            lowerCase: false,
            wordListSeparator: false,
            codes: false
        ), PART_ONLY_FILTER = new(
            partType: true,
            underscoreLength: false,
            underscoreInSentenceLength: false,
            index: false,
            questionNumberAlignment: false,
            markQuestionNumber: false,
            underscoreLengthFixed: false,
            provideCode: false,
            lowerCase: false,
            wordListSeparator: false,
            codes: false
        );

        public static readonly PartConfigNull NULL_CONFIG = new();
    }
}
