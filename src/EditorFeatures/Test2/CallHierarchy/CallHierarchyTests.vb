' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Threading.Tasks
Imports Microsoft.CodeAnalysis.Editor.UnitTests.Utilities
Imports Microsoft.CodeAnalysis.Navigation
Imports Microsoft.VisualStudio.Language.CallHierarchy

Namespace Microsoft.CodeAnalysis.Editor.UnitTests.CallHierarchy
    Public Class CallHierarchyTests
        <WpfFact, Trait(Traits.Feature, Traits.Features.CallHierarchy)>
        Public Sub TestScopes()
            Dim input =
<Workspace>
    <Project Language="C#" AssemblyName="Assembly1" CommonReferences="true">
        <Document>
namespace C
{
    public class CC
    {
        public int GetFive() { return 5; }
    }
}
        </Document>
        <Document>
using C;
namespace G
{
    public class G
    {
        public void G()
        {
            CC c = new CC();
            c.GetFive();
        }
    }
}

        </Document>
    </Project>
    <Project Language="C#" AssemblyName="Assembly2" CommonReferences="true">
        <ProjectReference>Assembly1</ProjectReference>
        <Document>
using C;
public class D
{
    void bar()
    {
        var c = new C.CC();
        var d = c.Ge$$tFive();
    }
}
        </Document>
        <Document>
using C;
public class DSSS
{
    void bar()
    {
        var c = new C.CC();
        var d = c.GetFive();
    }
}
        </Document>
    </Project>
</Workspace>

            Dim testState = CallHierarchyTestState.Create(input)
            Dim root = testState.GetRoot()
            testState.VerifyResult(root, String.Format(EditorFeaturesResources.Calls_To_0, "GetFive"), {"DSSS.bar()", "D.bar()", "G.G.G()"}, CallHierarchySearchScope.EntireSolution)
            Dim documents = testState.GetDocuments({"Test3.cs", "Test4.cs"})
            testState.VerifyResult(root, String.Format(EditorFeaturesResources.Calls_To_0, "GetFive"), {"DSSS.bar()", "D.bar()", "G.G.G()"}, CallHierarchySearchScope.CurrentProject)
            documents = testState.GetDocuments({"Test3.cs"})
            testState.VerifyResult(root, String.Format(EditorFeaturesResources.Calls_To_0, "GetFive"), {"D.bar()"}, CallHierarchySearchScope.CurrentDocument, documents)
        End Sub

        <WpfFact, Trait(Traits.Feature, Traits.Features.CallHierarchy)>
        Public Sub TestVBMethod()
            Dim input =
<Workspace>
    <Project Language="Visual Basic" AssemblyName="Assembly1" CommonReferences="true">
        <Document>
Class C
    Sub Foo()
        Fo$$o()
    End Sub
End Class
            </Document>
    </Project>
</Workspace>

            Dim testState = CallHierarchyTestState.Create(input)
            Dim root = testState.GetRoot()
            testState.VerifyResult(root, String.Format(EditorFeaturesResources.Calls_To_0, "Foo"), {"C.Foo()"})
        End Sub

        <WpfFact, Trait(Traits.Feature, Traits.Features.CallHierarchy)>
        Public Sub TestVBInterface()
            Dim input =
<Workspace>
    <Project Language="Visual Basic" AssemblyName="Assembly1" CommonReferences="true">
        <Document>
Class C
    Implements I
    Sub Foo() Implements I.Foo
        Foo()
    End Sub
End Class

Interface I
    Sub F$$oo()
End Interface
            </Document>
    </Project>
</Workspace>

            Dim testState = CallHierarchyTestState.Create(input)
            Dim root = testState.GetRoot()
            testState.VerifyResult(root, String.Format(EditorFeaturesResources.Implements_0, "Foo"), {"C.Foo()"})
        End Sub

        <WpfFact, Trait(Traits.Feature, Traits.Features.CallHierarchy)>
        Public Sub TestInterfaceScopes()
            Dim input =
<Workspace>
    <Project Language="C#" AssemblyName="Assembly1" CommonReferences="true">
        <Document>
namespace C
{
    public interface I
    {
        void fo$$o();
    }

    public class C : I
    {
        void foo() { }
    }
}
        </Document>
        <Document>
using C;
namespace G
{
    public Class G : I
    {
        public void foo()
        {
        }
    }
}

        </Document>
    </Project>
    <Project Language="C#" AssemblyName="Assembly2" CommonReferences="true">
        <ProjectReference>Assembly1</ProjectReference>
        <Document>
using C;
public class D : I
{
    public void foo()
    {
    }
}
        </Document>
    </Project>
</Workspace>

            Dim testState = CallHierarchyTestState.Create(input)
            Dim root = testState.GetRoot()
            testState.VerifyResult(root, String.Format(EditorFeaturesResources.Implements_0, "foo"), {"D.foo()", "G.G.foo()", "C.C.foo()"}, CallHierarchySearchScope.EntireSolution)
            Dim documents = testState.GetDocuments({"Test1.cs", "Test2.cs"})
            testState.VerifyResult(root, String.Format(EditorFeaturesResources.Implements_0, "foo"), {"G.G.foo()", "C.C.foo()"}, CallHierarchySearchScope.CurrentProject, documents)
            documents = testState.GetDocuments({"Test1.cs"})
            testState.VerifyResult(root, String.Format(EditorFeaturesResources.Implements_0, "foo"), {"C.C.foo()"}, CallHierarchySearchScope.CurrentDocument, documents)
        End Sub

        <WorkItem(981869, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/981869")>
        <WpfFact, Trait(Traits.Feature, Traits.Features.CallHierarchy)>
        Public Sub TestCallHierarchyCrossProjectNavigation()
            Dim input =
<Workspace>
    <Project Language="C#" AssemblyName="Assembly1" CommonReferences="true">
        <Document>
public interface IChangeSignatureOptionsService
{
    bool GetChangeS$$ignatureOptions();
}
        </Document>
    </Project>
    <Project Language="C#" AssemblyName="Assembly2" CommonReferences="true">
        <ProjectReference>Assembly1</ProjectReference>
        <Document>
using System;

class CSharpIt : IChangeSignatureOptionsService
{
    public bool GetChangeSignatureOptions()
    {
        throw new NotImplementedException();
    }
}
        </Document>
    </Project>
</Workspace>

            Dim testState = CallHierarchyTestState.Create(input)
            Dim root = testState.GetRoot()
            testState.SearchRoot(root,
                                 String.Format(EditorFeaturesResources.Implements_0, "GetChangeSignatureOptions"),
                                 Sub(c)
                                     Assert.Equal("Assembly2", c.Project.Name)
                                 End Sub,
                                 CallHierarchySearchScope.EntireSolution)
        End Sub

        <WorkItem(844613, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/844613")>
        <WpfFact, Trait(Traits.Feature, Traits.Features.CallHierarchy)>
        Public Sub TestMustInheritMethodInclusionToOverrides()
            Dim input =
<Workspace>
    <Project Language="Visual Basic" AssemblyName="Assembly1" CommonReferences="true">
        <Document>
Public MustInherit Class Base
    Public MustOverride Sub $$M()
End Class

Public Class Derived
    Inherits Base

    Public Overrides Sub M()
        Throw New NotImplementedException()
    End Sub
End Class
            </Document>
    </Project>
</Workspace>

            Dim testState = CallHierarchyTestState.Create(input)
            Dim root = testState.GetRoot()
            testState.VerifyResult(root, EditorFeaturesResources.Overrides_, {"Derived.M()"})
        End Sub

        <WorkItem(1022864, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/1022864")>
        <WpfFact, Trait(Traits.Feature, Traits.Features.CallHierarchy)>
        Public Sub TestNavigateCrossProject()
            Dim input =
    <Workspace>
        <Project Language="C#" AssemblyName="Assembly1" CommonReferences="true">
            <Document>
public class C
{
    public virtual void fo$$o() { }
}
        </Document>
        </Project>
        <Project Language="C#" AssemblyName="Assembly2" CommonReferences="true">
            <ProjectReference>Assembly1</ProjectReference>
            <Document>
class D : C
{
    public override void foo() { }
}
        </Document>
        </Project>
    </Workspace>

            Dim testState = CallHierarchyTestState.Create(input, GetType(MockSymbolNavigationServiceProvider))
            Dim root = testState.GetRoot()
            testState.Navigate(root, EditorFeaturesResources.Overrides_, "D.foo()")

            Dim mockNavigationService = DirectCast(testState.Workspace.Services.GetService(Of ISymbolNavigationService)(), MockSymbolNavigationServiceProvider.MockSymbolNavigationService)
            Assert.NotNull(mockNavigationService.TryNavigateToSymbolProvidedSymbol)
            Assert.NotNull(mockNavigationService.TryNavigateToSymbolProvidedProject)
        End Sub

        <WorkItem(1022864, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/1022864")>
        <WpfFact, Trait(Traits.Feature, Traits.Features.CallHierarchy)>
        Public Sub TestUseDocumentIdWhenNavigating()
            Dim input =
    <Workspace>
        <Project Language="C#" AssemblyName="Assembly1" CommonReferences="true">
            <Document>
namespace N
{
    class C
    {
        void F$$oo()
        {
        }
    }

    class G
    {
        void Main()
        {
            var c = new C();
            c.Foo();
        }
    }
}
        </Document>
        </Project>
    </Workspace>

            Dim testState = CallHierarchyTestState.Create(input, GetType(MockDocumentNavigationServiceProvider))
            Dim root = testState.GetRoot()
            testState.VerifyRoot(root, "N.C.Foo()", {String.Format(EditorFeaturesResources.Calls_To_0, "Foo")})
            testState.Navigate(root, String.Format(EditorFeaturesResources.Calls_To_0, "Foo"), "N.G.Main()")

            Dim navigationService = DirectCast(testState.Workspace.Services.GetService(Of IDocumentNavigationService)(), MockDocumentNavigationServiceProvider.MockDocumentNavigationService)
            Assert.NotEqual(navigationService.ProvidedDocumentId, Nothing)
            Assert.NotEqual(navigationService.ProvidedTextSpan, Nothing)
        End Sub

        <WorkItem(1098507, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/1098507")>
        <WpfFact, Trait(Traits.Feature, Traits.Features.CallHierarchy)>
        Public Sub TestDisplayErrorWhenNotOnMemberCS()
            Dim input =
    <Workspace>
        <Project Language="C#" AssemblyName="Assembly1" CommonReferences="true">
            <Document>
cla$$ss C
{
    void Foo()
    {
    }
}
        </Document>
        </Project>
    </Workspace>
            Dim testState = CallHierarchyTestState.Create(input)
            Dim root = testState.GetRoot()
            Assert.Null(root)
            Assert.NotNull(testState.NotificationMessage)
        End Sub

        <WorkItem(1098507, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/1098507")>
        <WpfFact, Trait(Traits.Feature, Traits.Features.CallHierarchy)>
        Public Sub TestDisplayErrorWhenNotOnMemberVB()
            Dim input =
    <Workspace>
        <Project Language="Visual Basic" AssemblyName="Assembly1" CommonReferences="true">
            <Document>
Class C
    Public Sub M()
    End Sub
End Cla$$ss
        </Document>
        </Project>
    </Workspace>
            Dim testState = CallHierarchyTestState.Create(input)
            Dim root = testState.GetRoot()
            Assert.Null(root)
            Assert.NotNull(testState.NotificationMessage)
        End Sub

    End Class

End Namespace
