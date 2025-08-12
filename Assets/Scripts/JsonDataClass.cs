using System;
using System.Collections.Generic;
[Serializable]
public class LyricWord
{
    public string start;
    public string end;
    public string text;
}


[Serializable]
public class WordListWrapper
{
    public List<LyricWord> words;
}