using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using ResumeBestMatchWebAPI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Http;

namespace ResumeBestMatchWebAPI.Controllers
{
    public class RBMApiController : ApiController
    {
        public StringBuilder text;
        public List<results> resList;
        public results res;
        public ApiResponseModel responseModel;

        public RBMApiController()
        {
            text = new StringBuilder();
            resList = new List<results>();
            responseModel = new ApiResponseModel();
            res = new results();
        }

        public HttpResponseMessage Get(string context, string category, decimal threshold,int noOfMatches, string inputPath)
        {
            if (context == null || category == null || threshold == 0 || noOfMatches == 0 || inputPath == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "All the fileds are manditory : context, category, threshold, noOfMatches and inputPath");
            }
            else {
                try
                {
                    responseModel = GetTextFromPDF(inputPath, category, context, noOfMatches);
                }
                catch(Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
                return Request.CreateResponse(HttpStatusCode.OK, responseModel);
            }
        }
        public ApiResponseModel GetTextFromPDF(string filePath, string category, string context, int noOfMatches)
        {
            int count = 0;
            DirectoryInfo dir = new DirectoryInfo(filePath);
            if (dir.Exists)
            {
                IEnumerable<FileInfo> filesList = dir.GetFiles("*.*", SearchOption.TopDirectoryOnly);
                IEnumerable<FileInfo> fileQuery = from file in filesList
                                                  where file.Extension == ((category == "resume") ? ".pdf" : (category == "JD" ? ".docx" : ".txt"))
                                                  orderby file.Name.ToString()
                                                  select file;
                foreach (FileInfo file in fileQuery)
                {
                    string fullFileNameTxt = System.IO.Path.Combine(file.DirectoryName, file.FullName);
                    if (File.Exists(fullFileNameTxt))
                    {
                        if (category == "resume")
                        {
                            using (PdfReader reader = new PdfReader(fullFileNameTxt))
                            {
                                //for (int i = 0; i <= reader.NumberOfPages; i++)
                                for (int i = 1; i <= noOfMatches; i++)
                                {
                                    text.Append(PdfTextExtractor.GetTextFromPage(reader, i));
                                    count = Regex.Matches(text.ToString(), context).Count;
                                    res = new results
                                    {
                                        id = i,
                                        score = "0.5",
                                        path = file.Name
                                    };
                                    resList.Add(res);
                                }
                                responseModel = new ApiResponseModel
                                {
                                    status = "success",
                                    count = count,
                                    results = resList
                                };
                            }
                        }
                        else if(category == "JD")
                        {
                            //code for .docx file read
                            responseModel = new ApiResponseModel
                            {
                                status = "No Code",
                                count = count,
                            };
                        }
                        else
                        {
                            //code for txt file read
                            responseModel = new ApiResponseModel
                            {
                                status = "No Code",
                                count = count,
                            };
                        }
                    }
                }
            }
            return responseModel;
        }
    }
}
