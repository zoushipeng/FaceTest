using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCardSystem.Recognizer
{
    public interface IRecognizer
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        bool Init();
        /// <summary>
        /// 反初始化
        /// </summary>
        /// <returns></returns>
        bool Uninit();       
        /// <summary>
        /// 获取版本
        /// </summary>
        /// <returns></returns>
        IntPtr GetVersion();
        /// <summary>
        /// 检测是否存在
        /// </summary>
        /// <returns></returns>
        bool CheckKey();        
    }
}
