Public Class frmFinalizarDevolucao
    Public finalizar As Boolean = False
    Public perda As Boolean = False

    Private Sub btnFinalizar_Click(sender As Object, e As EventArgs) Handles btnFinalizar.Click
        finalizar = True

        frmEmprestimo.final = True

        frmMensagem.lblTexto.Text = "Emprestimo finalizado com sucesso!" + vbCrLf + "O exemplar devolvido voltou a constar como disponivel."
        frmMensagem.Size = New Size(480, 220)
        frmMensagem.btnOk.Location = New Point(200, 130)
        frmMensagem.ShowDialog()
        Me.Close()
    End Sub

    Private Sub btnPerda_Click(sender As Object, e As EventArgs) Handles btnPerda.Click
        perda = True

        frmConfirma.lblTexto.Text = "O Leitor já possui um novo exemplar em mãos para a reposição?"
        frmConfirma.ShowDialog()

        If frmConfirma.DialogResult = Windows.Forms.DialogResult.Yes Then
            frmMensagem.lblTexto.Text = "O registro do exemplar perdido foi excluído, por favor, registrar o novo exemplar no sistema e atribuir a ele uma localização"
            frmMensagem.Size = New Size(950, 250)
            frmMensagem.btnOk.Location = New Point(450, 130)
            frmMensagem.ShowDialog()
        ElseIf frmConfirma.DialogResult = Windows.Forms.DialogResult.No Then
            frmMensagem.lblTexto.Text = "O leitor, ou seu responsável, caso este seja menor de 16 anos, são responsáveis pelo livro a partir do momento da efetuação do empréstimo " + vbCrLf + "              e por sua vez, caso haja extravio ou perda, são responsáveis pela reposição do livro, de assunto e valor equivalente ao extraviado." + vbCrLf + "                                          A reposição deve ser feita em até 15 dias após a data do prazo de devolução"
            frmMensagem.Size = New Size(1100, 250)
            frmMensagem.btnOk.Location = New Point(520, 130)
            frmMensagem.ShowDialog()
        End If
        Me.Close()
    End Sub

    Private Sub frmFinalizarDevolucao_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub
End Class