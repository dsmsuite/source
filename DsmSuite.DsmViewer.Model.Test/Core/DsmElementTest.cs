using DsmSuite.DsmViewer.Model.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace DsmSuite.DsmViewer.Model.Test.Core
{
    [TestClass]
    public class ElementTest
    {
        [TestMethod]
        public void WhenElementIsConstructedWithoutPropertiesThenElementAccordingInputArguments()
        {
            int elementId = 1;
            string elementName = "name1";
            string elementType = "file";
            DsmElement element = new DsmElement(elementId, elementName, elementType, null);
            Assert.AreEqual(elementId, element.Id);
            Assert.AreEqual(elementName, element.Name);
            Assert.AreEqual(elementType, element.Type);
            Assert.AreEqual(0, element.Order);
            Assert.AreEqual(elementName, element.Fullname);
            Assert.IsNotNull(element.Properties);
            Assert.AreEqual(0, element.Properties.Count);
        }

        [TestMethod]
        public void WhenElementIsConstructedWithPropertiesThenElementAccordingInputArguments()
        {
            Dictionary<string, string> elementProperties = new Dictionary<string, string>();
            elementProperties["annotation"] = "some text";
            elementProperties["version"] = "1.0";
            int elementId = 1;
            string elementName = "name1";
            string elementType = "file";
            DsmElement element = new DsmElement(elementId, elementName, elementType, elementProperties);
            Assert.AreEqual(elementId, element.Id);
            Assert.AreEqual(elementName, element.Name);
            Assert.AreEqual(elementType, element.Type);
            Assert.AreEqual(0, element.Order);
            Assert.AreEqual(elementName, element.Fullname);
            Assert.IsNotNull(element.Properties);
            Assert.AreEqual("some text", element.Properties["annotation"]);
            Assert.AreEqual("1.0", element.Properties["version"]);
        }

        [TestMethod]
        public void TestBuildElementHierarchyUsingInsertChildAtEnd()
        {
            int parentId = 1;
            string parentName = "parent";
            DsmElement parent = new DsmElement(parentId, parentName, "", null);
            Assert.AreEqual(null, parent.Parent);
            Assert.AreEqual(0, parent.Children.Count);

            int child1Id = 10;
            string child1Name = "child1";
            DsmElement child1 = new DsmElement(child1Id, child1Name, "", null);
            Assert.AreEqual(null, child1.Parent);

            parent.InsertChildAtEnd(child1);
            Assert.AreEqual("parent.child1", child1.Fullname);
            Assert.AreEqual("child1", child1.GetRelativeName(parent));

            Assert.AreEqual(1, parent.Children.Count);
            Assert.AreEqual(child1, parent.Children[0]);
            Assert.AreEqual(parent, child1.Parent);
            Assert.IsTrue(child1.IsRecursiveChildOf(parent));

            int child2Id = 100;
            string child2Name = "child2";
            DsmElement child2 = new DsmElement(child2Id, child2Name, "", null);
            Assert.AreEqual(null, child2.Parent);

            parent.InsertChildAtEnd(child2);
            Assert.AreEqual("parent.child2", child2.Fullname);
            Assert.AreEqual("child2", child2.GetRelativeName(parent));

            Assert.AreEqual(2, parent.Children.Count);
            Assert.AreEqual(child1, parent.Children[0]);
            Assert.AreEqual(child2, parent.Children[1]);
            Assert.AreEqual(parent, child2.Parent);
            Assert.IsTrue(child2.IsRecursiveChildOf(parent));

            int child3Id = 1000;
            string child3Name = "child3";
            DsmElement child3 = new DsmElement(child3Id, child3Name, "", null);
            Assert.AreEqual(null, child3.Parent);
            Assert.AreEqual(null, child3.Parent);

            parent.InsertChildAtEnd(child3);
            Assert.AreEqual("parent.child3", child3.Fullname);
            Assert.AreEqual("child3", child3.GetRelativeName(parent));

            Assert.AreEqual(3, parent.Children.Count);
            Assert.AreEqual(child1, parent.Children[0]);
            Assert.AreEqual(child2, parent.Children[1]);
            Assert.AreEqual(child3, parent.Children[2]);
            Assert.AreEqual(parent, child3.Parent);
            Assert.IsTrue(child3.IsRecursiveChildOf(parent));

            parent.RemoveChild(child1);
            Assert.AreEqual(null, child1.Parent);
            Assert.AreEqual(2, parent.Children.Count);
            Assert.AreEqual(child2, parent.Children[0]);
            Assert.AreEqual(child3, parent.Children[1]);
            Assert.IsFalse(child1.IsRecursiveChildOf(parent));

            parent.RemoveChild(child2);
            Assert.AreEqual(null, child2.Parent);
            Assert.AreEqual(1, parent.Children.Count);
            Assert.AreEqual(child3, parent.Children[0]);
            Assert.IsFalse(child2.IsRecursiveChildOf(parent));

            parent.RemoveChild(child3);
            Assert.AreEqual(null, child3.Parent);
            Assert.AreEqual(0, parent.Children.Count);
            Assert.IsFalse(child3.IsRecursiveChildOf(parent));
        }


        [TestMethod]
        public void TestBuildElementHierarchyUsingInsertChildAtIndex()
        {
            int parentId = 1;
            string parentName = "parent";
            DsmElement parent = new DsmElement(parentId, parentName, "", null);
            Assert.AreEqual(null, parent.Parent);
            Assert.AreEqual(0, parent.Children.Count);

            int child1Id = 10;
            string child1Name = "child1";
            DsmElement child1 = new DsmElement(child1Id, child1Name, "", null);
            Assert.AreEqual(null, child1.Parent);

            parent.InsertChildAtIndex(child1, 0);
            Assert.AreEqual("parent.child1", child1.Fullname);
            Assert.AreEqual("child1", child1.GetRelativeName(parent));

            Assert.AreEqual(1, parent.Children.Count);
            Assert.AreEqual(child1, parent.Children[0]);
            Assert.AreEqual(parent, child1.Parent);
            Assert.IsTrue(child1.IsRecursiveChildOf(parent));

            int child2Id = 100;
            string child2Name = "child2";
            DsmElement child2 = new DsmElement(child2Id, child2Name, "", null);
            Assert.AreEqual(null, child2.Parent);
            
            parent.InsertChildAtIndex(child2, 1);
            Assert.AreEqual("parent.child2", child2.Fullname);
            Assert.AreEqual("child2", child2.GetRelativeName(parent));

            Assert.AreEqual(2, parent.Children.Count);
            Assert.AreEqual(child1, parent.Children[0]);
            Assert.AreEqual(child2, parent.Children[1]);
            Assert.AreEqual(parent, child2.Parent);
            Assert.IsTrue(child2.IsRecursiveChildOf(parent));

            int child3Id = 1000;
            string child3Name = "child3";
            DsmElement child3 = new DsmElement(child3Id, child3Name, "", null);
            Assert.AreEqual(null, child3.Parent);
            Assert.AreEqual(null, child3.Parent);

            parent.InsertChildAtIndex(child3, 1);
            Assert.AreEqual("parent.child3", child3.Fullname);
            Assert.AreEqual("child3", child3.GetRelativeName(parent));

            Assert.AreEqual(3, parent.Children.Count);
            Assert.AreEqual(child1, parent.Children[0]);
            Assert.AreEqual(child3, parent.Children[1]);
            Assert.AreEqual(child2, parent.Children[2]);
            Assert.AreEqual(parent, child3.Parent);
            Assert.IsTrue(child3.IsRecursiveChildOf(parent));

            parent.RemoveChild(child1);
            Assert.AreEqual(null, child1.Parent);
            Assert.AreEqual(2, parent.Children.Count);
            Assert.AreEqual(child3, parent.Children[0]);
            Assert.AreEqual(child2, parent.Children[1]);
            Assert.IsFalse(child1.IsRecursiveChildOf(parent));

            parent.RemoveChild(child2);
            Assert.AreEqual(null, child2.Parent);
            Assert.AreEqual(1, parent.Children.Count);
            Assert.AreEqual(child3, parent.Children[0]);
            Assert.IsFalse(child2.IsRecursiveChildOf(parent));

            parent.RemoveChild(child3);
            Assert.AreEqual(null, child3.Parent);
            Assert.AreEqual(0, parent.Children.Count);
            Assert.IsFalse(child3.IsRecursiveChildOf(parent));
        }

        [TestMethod]
        public void TestContainChildWithName()
        {
            string childName = "the name";

            int parentId = 1;
            string parentName = "parent";
            DsmElement parent = new DsmElement(parentId, parentName, "", null);
            Assert.IsFalse(parent.ContainsChildWithName(childName));

            DsmElement child1 = new DsmElement(10, "Child 1", "", null);
            parent.InsertChildAtEnd(child1);
            Assert.IsFalse(parent.ContainsChildWithName(childName));

            DsmElement child2 = new DsmElement(11, "Child 2", "", null);
            parent.InsertChildAtEnd(child2);
            Assert.IsFalse(parent.ContainsChildWithName(childName));

            DsmElement theChild = new DsmElement(2, childName, "", null);
            parent.InsertChildAtEnd(theChild);
            Assert.IsTrue(parent.ContainsChildWithName(childName));
            Assert.IsFalse(parent.ContainsChildWithName(childName.ToUpper()));
            Assert.IsFalse(parent.ContainsChildWithName("the náme"));
        }

        [TestMethod]
        public void TestInsertElementIndexInRange()
        {
            int parentId = 1;
            string parentName = "parent";
            DsmElement parent = new DsmElement(parentId, parentName, "", null);
            Assert.AreEqual(0, parent.Children.Count);

            DsmElement child1 = new DsmElement(10, "child 1", "", null);
            parent.InsertChildAtIndex(child1, 0);
            Assert.AreEqual(1, parent.Children.Count);
            Assert.AreEqual(parent, child1.Parent);

            DsmElement child2 = new DsmElement(11, "child 2", "", null);
            parent.InsertChildAtIndex(child2, 0);
            Assert.AreEqual(2, parent.Children.Count);
            Assert.AreEqual(parent, child2.Parent);

            DsmElement child3 = new DsmElement(12, "child 3", "", null);
            parent.InsertChildAtIndex(child3, 2);
            Assert.AreEqual(3, parent.Children.Count);
            Assert.AreEqual(parent, child3.Parent);
        }

        [TestMethod]
        public void TestInsertElementOutOfRange()
        {
            int parentId = 1;
            string parentName = "parent";
            DsmElement parent = new DsmElement(parentId, parentName, "", null);
            Assert.AreEqual(0, parent.Children.Count);

            DsmElement child1 = new DsmElement(10, "child 1", "", null);
            parent.InsertChildAtIndex(child1, 1);
            Assert.AreEqual(1, parent.Children.Count);
            Assert.AreEqual(parent, child1.Parent);

            DsmElement child2 = new DsmElement(11, "child 2", "", null);
            parent.InsertChildAtIndex(child2, 1000*1000);
            Assert.AreEqual(2, parent.Children.Count);
            Assert.AreEqual(parent, child2.Parent);

            DsmElement child3 = new DsmElement(12, "child 3", "", null);
            parent.InsertChildAtIndex(child3, -1);
            Assert.AreEqual(3, parent.Children.Count);
            Assert.AreEqual(parent, child3.Parent);
        }
    }
}
