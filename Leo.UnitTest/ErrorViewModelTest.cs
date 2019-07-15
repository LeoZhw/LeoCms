using Microsoft.VisualStudio.TestTools.UnitTesting;
using LeoCms.Models;

namespace Leo.UnitTest
{
    [TestClass]
    public class ErrorViewModelTest
    {
        [TestMethod]
        public void TestRequestId()
        {
            //ÉùÃû
            ErrorViewModel error = new ErrorViewModel();
            //Ö´ÐÐ
            error.RequestId = "200";
            //¶ÏÑÔ
            Assert.AreEqual(error.RequestId, "200");
        }
    }
}
