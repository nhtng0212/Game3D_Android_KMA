using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace BlackMarket {
    public static class StoryMemory {
        [Serializable] class Record { public List<string> seen=new List<string>(); }
        static readonly Dictionary<string,Record> cache=new Dictionary<string,Record>();
        static Record Read(string save){
            if(cache.TryGetValue(save,out var record))return record;
            try {record=File.Exists(save+".reading")?JsonUtility.FromJson<Record>(File.ReadAllText(save+".reading")):new Record();}catch{record=new Record();}
            if(record==null || record.seen==null)record=new Record();cache[save]=record;return record;
        }
        public static bool Seen(string save,string id)=>!string.IsNullOrEmpty(id) && Read(save).seen.Contains(id);
        public static void Mark(string save,string id){
            if(string.IsNullOrEmpty(id))return;var r=Read(save);if(r.seen.Contains(id))return;r.seen.Add(id);
            try{Directory.CreateDirectory(Path.GetDirectoryName(save));File.WriteAllText(save+".reading.tmp",JsonUtility.ToJson(r));if(File.Exists(save+".reading"))File.Replace(save+".reading.tmp",save+".reading",null);else File.Move(save+".reading.tmp",save+".reading");}catch(Exception e){Debug.LogWarning("Không lưu được trạng thái đã đọc: "+e.Message);}
        }
    }
}
