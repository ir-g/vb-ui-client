Imports System.Threading
'Imports System.Web.Script.Serialization
Imports System
Imports System.Net
Imports System.IO
Imports System.Xml
Imports System.Text

Public Class Form1
    Const startupUrl = "http://localhost:3000/home"

    Dim heightMarker = 0
    Dim panelWidth = 0
    Dim makingReq = False
    Const breakHeight = 12
    Dim formData As New Dictionary(Of String, String)

    Function httpGetReq(sUrl As String)
        Dim data As String = ""
        Dim wrGETURL As WebRequest
        wrGETURL = WebRequest.Create(sUrl)

        Dim objStream As Stream
        objStream = wrGETURL.GetResponse.GetResponseStream()

        Dim objReader As New StreamReader(objStream)
        Dim sLine As String = ""
        Dim i As Integer = 0

        Do While Not sLine Is Nothing
            i += 1
            sLine = objReader.ReadLine
            If Not sLine Is Nothing Then
                'Console.WriteLine("{0}:{1}", i, sLine)
                data += sLine
            End If
        Loop
        'RichTextBox1.Text = httpGetReq("http://google.com")
        Return data
    End Function

    Function httpPostReq(sUrl As String, reqDict As Dictionary(Of String, String))
        Dim strId As String = "ss"
        Dim strName As String = "nn"

        Dim encoding = New ASCIIEncoding()
        Dim postData As String = "postReq=" + "true"
        'postData += ("&username=" + strName)
        For Each reqVal In reqDict
            postData += ("&" + reqVal.Key + "=" + reqVal.Value)
        Next reqVal
        Dim Data As Byte() = encoding.GetBytes(postData) 'Byte[]

        '// Prepare web request...
        Dim myRequest As HttpWebRequest = WebRequest.Create(sUrl)
        myRequest.Method = "POST"
        myRequest.ContentType = "application/x-www-form-urlencoded"
        myRequest.ContentLength = Data.Length
        Dim newStream As Stream = myRequest.GetRequestStream()
        '// Send the data.
        newStream.Write(Data, 0, Data.Length)
        newStream.Close()

        Dim objReader As New StreamReader(myRequest.GetResponse.GetResponseStream())
        Dim sLine As String = ""
        Dim i As Integer = 0
        Dim resData As String = ""
        Do While Not sLine Is Nothing
            i += 1
            sLine = objReader.ReadLine
            If Not sLine Is Nothing Then
                'Console.WriteLine("{0}:{1}", i, sLine)
                resData += sLine
            End If
        Loop
        'RichTextBox1.Text = httpGetReq("http://google.com")
        Return resData
    End Function

    Public Sub startUp()
        panelWidth = panel.Width
    End Sub

    Public Sub deleteChildren()
        panel.Controls.Clear()
    End Sub

    Sub addChildLabel(name As String, text As String)
        Dim child = New Label
        child.Name = name
        child.Text = text
        _DrawInChild(child)
    End Sub

    Sub addChildTextBox(name As String, shouldMaskData As Boolean)
        Dim child = New TextBox
        child.Name = name
        If shouldMaskData Then
            child.UseSystemPasswordChar = True
        End If
        _DrawInChild(child)
    End Sub

    Sub addChildBreak(isDoubleHeight As Boolean)
        heightMarker += breakHeight
        If isDoubleHeight Then
            heightMarker += breakHeight
        End If
    End Sub

    Sub addChildTitle(titleText As String)
        Me.Text = titleText
    End Sub

    Sub addChildButton(name As String, text As String)
        Dim child = New Button
        child.Name = name
        child.Text = text
        AddHandler child.Click, AddressOf HandleDynamicButtonClick
        _DrawInChild(child)
    End Sub

    Sub _DrawInChild(child)
        child.Width = panelWidth
        child.Top = heightMarker
        panel.Controls.Add(child)
        heightMarker += 24
    End Sub

    Sub HandleDynamicButtonClick(ByVal sender As Object, ByVal e As EventArgs)
        Dim btn As Button = DirectCast(sender, Button)
        If (formData.ContainsKey("testBit")) Then
            Console.WriteLine("Button " & btn.Name & "clicked")
            formData.Remove("testBit")
            onAction(btn.Name)
        Else
            MsgBox("Request already made")
        End If
    End Sub

    Sub endAddHandlers()
        For Each cControl As Control In panel.Controls
            If (TypeOf cControl Is Button) Then
                'Now SOLVED BY ADDING z
                'AddHandler cControl.KeyUp, AddressOf HandleDynamicButtonClick
            End If

        Next cControl


    End Sub

    Sub onAction(actionName As String)
        If (makingReq = True) Then
            Exit Sub
        End If
        makingReq = True
        Dim cControl As Control
        For Each cControl In panel.Controls
            If (TypeOf cControl Is TextBox) Then
                formData.Add(cControl.Name, cControl.Text)
            End If
        Next cControl
        'formData.Add("action", actionName)
        Dim queryString = "?"
        ' For Each queryItem As KeyValuePair(Of String, String) In formData
        'queryString &= queryItem.Key & "=" & queryItem.Value & "&"
        'Next queryItem
        heightMarker = 0
        Dim fetchedData = httpPostReq("http://localhost:3000/" & actionName, formData)
        formData.Clear()
        Console.WriteLine(fetchedData)
        startInterface(False, fetchedData)
        Console.WriteLine(queryString)
        'HTTP data submission
        makingReq = False
    End Sub

    Sub handleMarkupCommand(line As String())
        Dim baseCommand As String = line(0)
        Select Case (baseCommand)
            Case "title"
                Console.WriteLine("Setting title: " & line(1))
                'Title
                addChildTitle(line(1))
            Case "label"
                Console.WriteLine("Adding label: " & line(1))
                'Name of label, Label contents
                addChildLabel(line(1), line(2))
            Case "break"
                Console.WriteLine("Adding break")
                'Is break double height
                If (line(1) = "true") Then
                    addChildBreak(True)
                Else
                    addChildBreak(True)
                End If
            Case "button"
                Console.WriteLine("Adding button: " & line(1))
                'Name of button, Button text
                addChildButton(line(1), line(2))

            Case "textbox"
                Console.WriteLine("Adding textbox: " & line(1))
                Dim shouldMaskData As Boolean
                If (line(2) = "true") Then
                    shouldMaskData = True
                Else
                    shouldMaskData = False
                End If
                addChildTextBox(line(1), line(2))
            Case Else
                Console.WriteLine("Error. Component '" & baseCommand & "' not found.")
        End Select
    End Sub

    Sub startInterface(isUrl As Boolean, funcdata As String)
        Dim responseData As String = funcdata
        If (isUrl) Then
            Try
                responseData = httpGetReq(funcdata)
            Catch ex As Exception
                MsgBox("Error fetching data from " & funcdata & vbCrLf & "Closing app.")
                Me.Close()
            End Try
        End If
        formData.Add("testBit", "test")
        Console.WriteLine(responseData)
        deleteChildren()
        Dim newLineSet = "|" 'vbCrLf
        Dim arrLines As String() = responseData.Split(newLineSet)
        For Each lineData In arrLines
            Dim line As String() 'Array of strings
            line = lineData.Split(",")
            Console.WriteLine(line(0))
            handleMarkupCommand(line)
        Next
        panel.Width = panelWidth
        endAddHandlers()
    End Sub


    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.MaximumSize = New Size(400, 400)
        Me.MinimumSize = Me.MaximumSize
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        panel.AutoScroll = True
        panel.VerticalScroll.Visible = False
        startUp()
        startInterface(True, startupUrl)
        'addChildLabel("lbl1", "Welcome To TestUiGen")
        'addChildLabel("lbl2", "The value Of Me.Top() Is " & Me.Top)
        'addChildBreak(False)
        'addChildLabel("lbl3", "Please enter your username")
        'addChildTextBox("username", False)
        'addChildLabel("lbl3", "Please enter your password")
        'addChildTextBox("password", True)
        'addChildButton("btn1", "Go")
        'addChildBreak(True)
        'addChildBreak(True)
        'addChildBreak(True)
        'addChildBreak(True)
        'addChildLabel("lbl3", "Please enter your password")
        'addChildBreak(True)
        'addChildLabel("lbl3", "Please enter your password")
        'addChildLabel("lbl3", "Please enter your password")
        'addChildLabel("lbl3", "Please enter your password")
        'addChildLabel("lbl3", "Please enter your password")
        'addChildLabel("lbl3", "Please enter your password")
        'addChildLabel("lbl3", "Please enter your password")
        'addChildLabel("lbl3", "Please enter your password")
        'addChildBreak(True)
        'addChildBreak(True)
        'addChildBreak(True)
        'addChildButton("btn2", "Go2")
        'Thread.Sleep(5000)
        'deleteChildren()
        endAddHandlers()
    End Sub
End Class
