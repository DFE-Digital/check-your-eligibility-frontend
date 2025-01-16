using CheckYourEligibility_FrontEnd.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;

namespace CheckYourEligibility_FrontEnd.UseCases
{
    public interface IAddChildUseCase
    {
        bool ExecuteAsync(Children request, ITempDataDictionary tempData);
    }

    public class AddChildUseCase : IAddChildUseCase
    {
        private const int MaxChildren = 99;

        public bool ExecuteAsync(Children request, ITempDataDictionary tempData)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (tempData == null)
            {
                throw new ArgumentNullException(nameof(tempData));
            }

            if (request.ChildList.Count >= MaxChildren)
            {
                return false;
            }

            request.ChildList.Add(new Child());

            tempData["IsChildAddOrRemove"] = true;
            tempData["ChildList"] = JsonConvert.SerializeObject(request.ChildList);

            return true;
        }
    }
}