using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyPageBaseTest.Mocks
{
    class TestHelp : AbstractHelp
    {
        public List<string> KeywordsDisplayed = new List<string>();

        public override void DisplayTopicFromF1Keyword(string pszKeyword)
        {
            KeywordsDisplayed.Add(pszKeyword);
        }
    }
}
