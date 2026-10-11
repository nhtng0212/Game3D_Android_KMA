using System.Collections.Generic;
namespace BlackMarket {
    // Two automatic phone subtitles; legacy choice fields retain source compatibility.
    public static class PhoneDialogue {
        public class Choice { public string text,next,effect; public Choice(string text,string next,string effect=""){this.text=text;this.next=next;this.effect=effect;} }
        public class Node {public string speaker,text;public Choice[] choices;public Node(string speaker,string text,params Choice[] choices){this.speaker=speaker;this.text=text;this.choices=choices;} }
        public static readonly Dictionary<string,Node> Nodes=new Dictionary<string,Node>{
            {"start",new Node("MARCUS","Alex, nghe chú… đừng tin bất kỳ ai biết đến cái tên North Point.")},
            {"warning",new Node("MARCUS","Và dù có chuyện gì xảy ra, cũng đừng mở Cửa 006!")}
        };
        public static void Apply(CampaignSave state,string effect){if(effect=="card")state.askedAboutCard=true;if(effect=="hunters")state.askedAboutHunters=true;if(effect=="promise")state.promisedMarcus=true;}
    }
}
