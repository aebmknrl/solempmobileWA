using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Libreria
{
    public class LastError
    {
        public int ErrorNo = 0;
        public string ErrorMsg = "";
        public bool isCustomError = false;
        public string ExtraInfo = "";
        public string className = "";
        public string methodName = "";
        public string source = "";
        public int lineNo = 0;


        // Limpia las propiedades de la Clase
        public void Clean()
        {
            this.ErrorNo = 0;
            this.ErrorMsg = "";
            this.isCustomError = false;
            this.ExtraInfo = "";
            this.className = "";
            this.methodName = "";
            this.source = "";
            this.lineNo = 0;
        }


    // Lanza el error al manejador de errores
    public Exception goThrow(string Msg)
        {
            this.isCustomError = true;
            throw new ArgumentException(Msg);
        }

        // Llena el objeto con los valores del error
        public void initWithException(Exception e)
        {
            this.ErrorNo = e.HResult;
            this.ErrorMsg = e.Message.Replace(@"\", "/");
            this.source = e.Source.Replace(@"\","/");
            this.lineNo = new System.Diagnostics.StackTrace(e, true).GetFrame(0).GetFileLineNumber();
            this.ExtraInfo = "";
            
            


            if (e.Data.Count == 0)
            {
                return;
            }


            // Construye la informacion Extra del error

            foreach (DictionaryEntry de in e.Data)
            {
                this.ExtraInfo = this.ExtraInfo +
                                 de.Key.ToString() + ": " +
                                 de.Value.ToString().Trim();
            }

        }


        public void initWithException(Exception e, string cClassName, string cMethodName)
        {
            this.className = cClassName;
            this.methodName = cMethodName;
            initWithException(e);
        }


        // Retorna informacion del error en un JSON
        public string ToJSON()
        {
            // Construye el JSON que se va a enviar como resultado del Error


            string customError = this.isCustomError ? "true" : "false";

            string cJSON = "{" +
                ClasesComunes.EntreComillas("Result") + ":" +
                ClasesComunes.EntreComillas("ERROR") + "," +
                ClasesComunes.EntreComillas("Data") + ":" +
                ClasesComunes.EntreComillas("") + "," +
                ClasesComunes.EntreComillas("Error") + ":" +
                "{" +
                    ClasesComunes.EntreComillas("ErrorNo") + ":" + this.ErrorNo.ToString().Trim() + "," +
                    ClasesComunes.EntreComillas("ErrorMsg") + ":" + ClasesComunes.EntreComillas(this.ErrorMsg.ToString().Trim()) + "," +
                    ClasesComunes.EntreComillas("isCustomError") + ":" + customError + "," +
                    ClasesComunes.EntreComillas("Class") + ":" + ClasesComunes.EntreComillas(this.className) + "," +
                    ClasesComunes.EntreComillas("Method") + ":" + ClasesComunes.EntreComillas(this.methodName) + "," +
                    ClasesComunes.EntreComillas("Source") + ":" + ClasesComunes.EntreComillas(this.source) + "," +
                    ClasesComunes.EntreComillas("LineNo") + ":" + this.lineNo.ToString().Trim() + "," +
                    ClasesComunes.EntreComillas("ExtraInfo") + ":" + ClasesComunes.EntreComillas(this.ExtraInfo) +
                "}" +
                           "}";



            return cJSON;
        }


        public string ToJSON(Exception e)
        {
            initWithException(e);
            return ToJSON();
        }

        public string ToJSON(Exception e, string cClassName, string cMethodName)
        {
            initWithException(e, cClassName, cMethodName);
            return ToJSON(e);
        }


    }
}
