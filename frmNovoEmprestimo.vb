Imports System.Data.SqlClient
Public Class frmNovoEmprestimo

    Dim VLeitor As Double
    Dim VLivro As Double
    Private con As SqlConnection = Biblioteca.Conexao()
    Public codigo As Integer
    Public dtbLeitor As New DataTable()
    Public dtbLivro As New DataTable()

    Public Sub iniciarTexto()
        txtCarteirinha.Text = ""
        txtISBN.Text = ""
    End Sub

    Private Sub btnOk_Click(sender As Object, e As EventArgs) Handles btnOk.Click

        con.Open()
        Dim adp As New SqlDataAdapter("SELECT Codigo FROM Livro WHERE ISBN = @ISBN", con)
        adp.SelectCommand.Parameters.Add("@ISBN", SqlDbType.VarChar, 17).Value = txtISBN.Text
        dtbLivro.Clear()
        adp.Fill(dtbLivro)

        If dtbLivro.Rows.Count > 0 Then
            DialogResult = Windows.Forms.DialogResult.OK
            codigo = dtbLivro.Rows(0).Item("Codigo")
        Else
            frmMensagem.lblTexto.Text = "ISBN informado não existe"
            frmMensagem.ShowDialog()
        End If

        con.Close()

    End Sub

    Private Sub btnCancelar_Click(sender As Object, e As EventArgs) Handles btnCancelar.Click
        DialogResult = Windows.Forms.DialogResult.Cancel
        frmEmprestimo.aux = True
    End Sub

    Private Sub frmNovoEmprestimo_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub

    Private Sub txtCarteirinha_TextChanged(sender As Object, e As EventArgs) Handles txtCarteirinha.TextChanged
        Dim aux As Integer
        aux = Len(txtCarteirinha.Text)
        If (aux < 1) Then
            VLeitor = False
        Else

            VLeitor = True
        End If
    End Sub

    Private Sub txtISBN_TextChanged(sender As Object, e As EventArgs) Handles txtISBN.TextChanged
        Dim aux As Integer
        aux = Len(txtISBN.Text)
        If (aux < 17) Then
            VLivro = False
        Else
            VLivro = True
        End If
    End Sub

    Private Sub btnOk_EnabledChanged(sender As Object, e As EventArgs) Handles btnOk.EnabledChanged, txtISBN.TextChanged, txtCarteirinha.TextChanged
        If VLivro = True And VLeitor = True Then
            btnOk.Enabled = True
        Else
            btnOk.Enabled = False
        End If
    End Sub

    Private Sub lblSair_Click(sender As Object, e As EventArgs) 
        Me.Close()
    End Sub
End Class