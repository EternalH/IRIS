using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// IRIS·Manager
/// </summary>
namespace IRIS.Manager
{
    /// <summary>
    /// 音乐管理
    /// </summary>
    public class MusicManager : MonoBehaviour
    {
        enum MusicName
        {
            Addiction = 0,
            WingsOfPiano = 1,
        }

        /// <summary>
        /// 名字索引
        /// </summary>
        public int NameIndex { get; private set; }
        /// <summary>
        /// 音乐名字
        /// </summary>
        public string _Name { get; private set; }

        /// <summary>
        /// 选择音乐
        /// </summary>
        /// <param name="index">歌曲索引</param>
        /// <returns>name</returns>
        public string SelectMusic(int index)
        {
            switch(index)
            {
                case 0:
                    NameIndex = (int)MusicName.Addiction;
                    _Name = "Addiction";
                    break;
                case 1:
                    NameIndex = (int)MusicName.WingsOfPiano;
                    _Name = "WingsOfPiano";
                    break;
            }

            return _Name;
        }
    }
}

