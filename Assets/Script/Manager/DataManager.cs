using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// IRIS·Manager
/// </summary>
namespace IRIS.Manager
{
    /// <summary>
    /// 数据管理
    /// </summary>
    public class DataManager : MonoBehaviour
    {
        private int maxScore;
        private Sorter<int> sorter = new Sorter<int>();

        /// <summary>
        /// 分数排序
        /// </summary>
        /// <param name="scores">分数列表</param>
        /// <returns>newScore</returns>
        public void ScoresSort(List<int> scores)
        {
            sorter.QuickSort(scores, 0, scores.Count);
            maxScore = scores[scores.Count];

        }

        /// <summary>
        /// 获取最高分数
        /// </summary>
        /// <returns>maxScore</returns>
        public int GetMaxScore()
        {
            return maxScore;
        }
    }

    /// <summary>
    /// 排序类泛型
    /// </summary>
    /// <typeparam name="T">泛型</typeparam>
    class Sorter<T> where T : IComparable
    {
        /// <summary>
        /// 快速排序法
        /// </summary>
        /// <param name="array">数组</param>
        /// <param name="low">数组首地址</param>
        /// <param name="high">数组尾地址</param>
        public void QuickSort(List<int> scores, int low, int high)
        {
            if (low >= high)
            {
                return;
            }
            int first = low, last = high;

            int key = scores[low];
            while (first < last)
            {
                while (first < last && scores[last].CompareTo(key) >= 0)
                {
                    last--;
                }
                scores[first] = scores[last];
                while (first < last && scores[first].CompareTo(key) <= 0)
                {
                    first++;
                }
                scores[last] = scores[first];
            }
            scores[first] = key;
            //递归排序数组左边的元素            
            QuickSort(scores, low, first - 1);
            //递归排序右边的元素  
            QuickSort(scores, first + 1, high);
        }

    }
}
