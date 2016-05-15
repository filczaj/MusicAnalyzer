using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicAnalyzer.Models
{
    public enum Match
    {
        None = 0,
        Poor = 1,
        Medium = 2,
        Good = 3,
        Perfect = 4
    };

    public enum ChordType
    {
        Tonic,
        Subdominant,
        Dominant,
        SecondStep,
        SixthStep,
        ThirdStep,
        Other
    };

    public enum ChordMode
    {
        Major,
        Minor,
        Enlarged,
        Diminished,
        Dominant,
        SeventhDim,
        Other
    };

    public enum ChordPriority
    {
        Tonic = 6,
        Subdominant = 4,
        Dominant = 4,
        SecondStep = 2,
        SixthStep = 3,
        ThirdStep = 2,
        SeventhStem = 0,
        Default = 0
    };

    public enum MeasureBeats
    {
        Begin = 4,
        Strong = 2,
        Weak = 1
    };

    public enum Interval
    { 
        Prime = 0,
        SecondLow = 1,
        Second = 2,
        TerceLow = 3,
        Terce = 4,
        Quart = 5,
        Triton = 6,
        Fith = 7,
        SixthLow = 8,
        Sixth = 9,
        SeventhLow = 10,
        Seventh = 11,
        Octave = 12
    }
}
