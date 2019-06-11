Imports System.Data.SqlClient
Imports System.Net.Mail
Imports System.Threading


Public Class frmEmprestimo

    Dim con As SqlConnection = Biblioteca.Conexao()

    Public dtb As New DataTable()
    Public dtbLeitor As New DataTable()
    Public dtbLivro As New DataTable()
    Public dtbEmprestimo As New DataTable()

    Dim bloqueio As Boolean
    Dim disponivel As Boolean
    Dim halivro As Boolean = False
    Dim haleitor As Boolean = False
    Public final As Boolean = False

    Dim mde As Boolean = False

    Public aux As Boolean

    Dim email As String
    Dim SmtpServer As New SmtpClient()
    Dim mail As New MailMessage()
    Dim motivoEmail As String
    Dim motivoBanco As String

    Dim atrasoEmail As Thread
    Dim empretimoEmail As Thread
    Dim devolucaoEmail As Thread
    Dim bloqueioEmail As Thread

    Public Sub AtualizarGrid()
        con.Open()
        Dim adp As New SqlDataAdapter("SELECT le.codigo AS [codigo leitor], le.bloqueio AS [bloqueio], e.codigo AS [Codigo Emprestismo], le.Nome AS [Nome do Leitor], le.email AS [Email], li.Nome AS [Nome do Livro], e.dataEmprestimo AS [Data do Emprestimo], e.dataDevolucao AS [Prazo da Devolução], e.Entregue AS [Devolvido] FROM Emprestimo e INNER JOIN Livro li ON  li.codigo = e.codLivro INNER JOIN Leitor le ON le.codigo = e.codLeitor WHERE e.entregue = 'false'", con)
        dtb.Clear()
        adp.Fill(dtb)
        con.Close()
        atrasoDevolucao()
    End Sub

    Public Sub leitor()
        Dim verificacao As New SqlDataAdapter("SELECT nome, codigo, bloqueio AS [Bloqueio], email AS [Email] FROM leitor WHERE codigo = @codLeitor", con)
        verificacao.SelectCommand.Parameters.Add("@codLeitor", SqlDbType.Int).Value = CInt(frmNovoEmprestimo.txtCarteirinha.Text)
        dtbLeitor.Clear()
        verificacao.Fill(dtbLeitor)
        If dtbLeitor.Rows.Count > 0 Then
            haleitor = True
            Dim usuario As String = dtbLeitor.Rows(0).Item("nome")
            email = dtbLeitor.Rows(0).Item("Email")
            bloqueio = dtbLeitor.Rows(0).Item("Bloqueio")
        ElseIf dtbLeitor.Rows.Count <= 0 Then
            ''POR O FORM DE MSG AQUI
            haleitor = False
            frmMensagem.lblTexto.Text = "NÃO HA UM LEITOR REGISTRADO COM ESTE NUMERO DE CARTEIRINHA."
            frmMensagem.Size = New Size(480, 250)
            frmMensagem.btnOk.Location = New Point(200, 130)
            frmMensagem.ShowDialog()

        End If
    End Sub

    Public Sub livro()
        Dim adp As New SqlDataAdapter("SELECT Disponibilidade AS [Disponibilidade], quant AS [Quantidade] FROM Livro WHERE ISBN = @ISBN", con)
        adp.SelectCommand.Parameters.Add("@ISBN", SqlDbType.VarChar, 17).Value = frmNovoEmprestimo.txtISBN.Text
        ''mudar para inserrção do isbn 
        dtbLivro.Clear()
        adp.Fill(dtbLivro)
        If dtbLivro.Rows.Count > 0 Then
            halivro = True
            disponivel = dtbLivro.Rows(0).Item("Disponibilidade")
        ElseIf dtbLivro.Rows.Count < 0 Then
            ''POR O FORM DE MSG AQUI
            frmMensagem.lblTexto.Text = "NÃO HA UM LIVRO REGISTRADO COM ESTE codigo // mudar para isbn "
            frmMensagem.Size = New Size(480, 250)
            frmMensagem.btnOk.Location = New Point(200, 130)
            frmMensagem.ShowDialog()
        End If
    End Sub

    Private Sub atrasoDevolucao()
        con.Open()
        For i = 0 To dtb.Rows.Count - 1 Step 1
            Dim x As Date = dtb.Rows(i).Item("Prazo da Devolução")
            If Today > x Then
                If dtb.Rows(i).Item("bloqueio") = False Then
                    motivoEmail = "Prezado leitor, gostariamos de lembrar que como usuario de nossa biblioteca é seu dever devolver os livros no prazo estipulado na efetuação do emprestimo. Devido ao descumprimento deste dever e do regulamento da biblioteca, vimos por meio deste informar que o(a) senhor(a) estara bloqueado e inaabito paro o uso da biblioteca ate que comparaça ao nosso recinto para o esclarecimento de toda situação, " + vbCrLf + "Atenciosamente, " + vbCrLf + " a Gerencia."
                    motivoBanco = "Atraso na Devolução"
                    email = dtb.Rows(i).Item("Email")
                    dtgEmprestimo.Rows(i).DefaultCellStyle.BackColor = Color.Red
                    Dim cmd As New SqlCommand("UPDATE Leitor SET Bloqueio = 'True', motivoBloqueio = @motivo WHERE codigo = @codLeitor", con)
                    cmd.Parameters.Add("@codLeitor", SqlDbType.Int).Value = dtb.Rows(i).Item("codigo leitor")
                    cmd.Parameters.Add("@motivo", SqlDbType.VarChar, 400).Value = motivoBanco
                    cmd.ExecuteNonQuery()
                    mde = False
                    atrasoEmail = New Thread(AddressOf mandaEmail)
                    atrasoEmail.Start()
                End If
                dtgEmprestimo.Rows(i).DefaultCellStyle.BackColor = Color.Red
            End If
        Next
        con.Close()
    End Sub

    Public Sub mandaEmail()

        Try
            If My.Computer.Network.Ping("www.google.com.br") Then

                SmtpServer.UseDefaultCredentials = False
                SmtpServer.Credentials = New Net.NetworkCredential("libraryscarter@gmail.com", "teste2016")
                SmtpServer.Host = "smtp.gmail.com"
                SmtpServer.Port = 587 'server para Hotmail
                SmtpServer.EnableSsl = True
                mail.From = New MailAddress("libraryscarter@gmail.com")
                mail.To.Add(email)
                mail.Body = motivoEmail
                mail.Subject = "Library's Carter - Bloqueio"

                SmtpServer.Send(mail)
                ''fazeer teste da conexao da internet antes de enviar email
                'If mde = True Then
                frmMensagem.lblTexto.Text = "Email enviado com sucesso!"
                frmMensagem.Size = New Size(380, 200)
                frmMensagem.btnOk.Location = New Point(150, 130)
                frmMensagem.ShowDialog()
            End If
        Catch a As Exception
            frmMensagem.lblTexto.Text = "Falha ao enviar e-mail."
                frmMensagem.Size = New Size(380, 250)
                frmMensagem.btnOk.Location = New Point(150, 130)
                frmMensagem.ShowDialog()
            End Try
        

    End Sub

    'leitor nao encontrado X 
    'livro nao encontrado X

    'RENOVAR
    ''''''''limite de renovação 
    'CONSULTAR 
    ''''''''busca nao encontrada

    'outro descritivo------------------------------------------
    ''DEVOLVER LIVRO

    ''EXIBIR TELA PARA BLOQUEIO DO LEITOR NO FORM DE DEVOLUÇÃO

    Private Sub txtBusca_TextChanged(sender As Object, e As EventArgs) Handles txtBusca.TextChanged, rdbNomeLivro.CheckedChanged, rdbNomeLeitor.CheckedChanged
        con.Open()

        If txtBusca.Text = "" Then
            Dim adp As New SqlDataAdapter("SELECT le.codigo AS [codigo leitor], le.bloqueio AS [bloqueio], e.codigo AS [Codigo Emprestismo], le.Nome AS [Nome do Leitor], le.email AS [Email], li.Nome AS [Nome do Livro], e.dataEmprestimo AS [Data do Emprestimo], e.dataDevolucao AS [Prazo da Devolução], e.Entregue AS [Devolvido] FROM Emprestimo e INNER JOIN Livro li ON  li.codigo = e.codLivro INNER JOIN Leitor le ON le.codigo = e.codLeitor WHERE e.entregue = 'false'", con)
            dtb.Clear()
            adp.Fill(dtb)
        End If

        If rdbNomeLivro.Checked Then
            Dim busca As New SqlDataAdapter("SELECT le.codigo AS [codigo leitor], le.bloqueio AS [bloqueio], e.codigo AS [Codigo Emprestismo], le.Nome AS [Nome do Leitor], le.email AS [Email], li.Nome AS [Nome do Livro], e.dataEmprestimo AS [Data do Emprestimo], e.dataDevolucao AS [Prazo da Devolução], e.Entregue AS [Devolvido] FROM Emprestimo e INNER JOIN Livro li ON  li.codigo = e.codLivro INNER JOIN Leitor le ON le.codigo = e.codLeitor WHERE e.entregue = 'false' and li.Nome like '%" & txtBusca.Text & "%'", con)
            dtb.Clear()
            busca.Fill(dtb)
        ElseIf rdbNomeLeitor.Checked Then
            Dim busca As New SqlDataAdapter("SELECT le.codigo AS [codigo leitor], le.bloqueio AS [bloqueio], e.codigo AS [Codigo Emprestismo], le.Nome AS [Nome do Leitor], le.email AS [Email], li.Nome AS [Nome do Livro], e.dataEmprestimo AS [Data do Emprestimo], e.dataDevolucao AS [Prazo da Devolução], e.Entregue AS [Devolvido] FROM Emprestimo e INNER JOIN Livro li ON  li.codigo = e.codLivro INNER JOIN Leitor le ON le.codigo = e.codLeitor WHERE e.entregue = 'false' and le.Nome like '%" & txtBusca.Text & "%'", con)
            dtb.Clear()
            busca.Fill(dtb)
        End If

        con.Close()
    End Sub

    Private Sub fmrEmprestimo_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        rdbNomeLeitor.Checked = True
        '' mostrar so os itens q entregue igual a fale
        dtgEmprestimo.DataSource = dtb
        AtualizarGrid()
        dtgEmprestimo.Columns("Devolvido").Visible = False
        dtgEmprestimo.Columns("codigo leitor").Visible = False
        dtgEmprestimo.Columns("email").Visible = False
        dtgEmprestimo.Columns("bloqueio").Visible = False

        dtgEmprestimo.Columns("Codigo Emprestismo").AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        dtgEmprestimo.Columns("Nome do Leitor").AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        dtgEmprestimo.Columns("Nome do Livro").AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        dtgEmprestimo.Columns("Data do Emprestimo").AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        dtgEmprestimo.Columns("Prazo da Devolução").AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill

        atrasoDevolucao()

    End Sub

    Private Sub btnNovoEmprestimo_Click(sender As Object, e As EventArgs) Handles btnNovoEmprestimo.Click
        aux = False
        While aux = False
            frmNovoEmprestimo.iniciarTexto()
            frmNovoEmprestimo.ShowDialog()

            If frmNovoEmprestimo.DialogResult = Windows.Forms.DialogResult.OK Then
                con.Open()

                leitor()
                livro()

                If disponivel = True Then

                    If bloqueio = False And haleitor = True Then

                        Dim cmd As New SqlCommand("INSERT INTO emprestimo VALUES (@livro, @leitor,GETDATE(), NULL, @devolução, @reposto)", con)
                        cmd.Parameters.Add("@livro", SqlDbType.Int).Value = frmNovoEmprestimo.codigo
                        cmd.Parameters.Add("@leitor", SqlDbType.Int).Value = CInt(frmNovoEmprestimo.txtCarteirinha.Text)
                        cmd.Parameters.Add("@devolução", SqlDbType.Bit).Value = 0
                        cmd.Parameters.Add("@reposto", SqlDbType.Bit).Value = 0
                        cmd.ExecuteNonQuery()

                        mde = True
                        empretimoEmail = New Thread(AddressOf mandaEmail)
                        empretimoEmail.Start()
                        aux = True
                    ElseIf bloqueio = True And haleitor = True Then
                        ''frm de msg aqui
                        frmMensagem.lblTexto.Text = "ESTE LEITOR ESTA BLOQUEADO"
                        frmMensagem.Size = New Size(380, 250)
                        frmMensagem.btnOk.Location = New Point(150, 130)
                        frmMensagem.ShowDialog()
                        aux = False
                    End If

                ElseIf halivro = True And disponivel = False Then
                    ''form de msg aqui 
                    frmMensagem.lblTexto.Text = "ESTE LIVRO NAO ESTA DISPONIVEL NO MOMENTO"
                    frmMensagem.Size = New Size(380, 250)
                    frmMensagem.btnOk.Location = New Point(150, 130)
                    frmMensagem.ShowDialog()
                    aux = False
                End If

                con.Close()
                AtualizarGrid()
            End If
        End While
    End Sub

    Private Sub btnRenovarEmprestimo_Click(sender As Object, e As EventArgs) Handles btnRenovarEmprestimo.Click
        Try
            If dtgEmprestimo.SelectedRows.Count > 0 Then
                frmRenovarEmprestimo.txtCodigoEmprestimo.Text = dtb.Rows(dtgEmprestimo.CurrentRow.Index).Item("Codigo Emprestismo")
            Else
                frmRenovarEmprestimo.txtCodigoEmprestimo.Text = ""
            End If
        Catch a As Exception
        End Try

        frmRenovarEmprestimo.ShowDialog()

        If frmRenovarEmprestimo.DialogResult = Windows.Forms.DialogResult.OK Then
            con.Open()

            Dim adp As New SqlDataAdapter("Select e.dataDevolucao As [Prazo da Devolução], l.bloqueio As [bloqueio], e.dataEmprestimo As [Data Do Emprestimo] FROM emprestimo e INNER JOIN Leitor l On L.codigo = e.codLeitor WHERE e.codigo = @codigo", con)
            adp.SelectCommand.Parameters.Add("@codigo", SqlDbType.Int).Value = CInt(frmRenovarEmprestimo.txtCodigoEmprestimo.Text)
            dtbEmprestimo.Clear()
            adp.Fill(dtbEmprestimo)
            con.Close()

            Dim bloqueio As Boolean = dtbEmprestimo.Rows(0).Item("bloqueio")

            If bloqueio = False Then
                Dim x As Date = dtbEmprestimo.Rows(0).Item("Data Do Emprestimo")
                Dim datadevolver As Date = dtbEmprestimo.Rows(0).Item("Prazo da Devolução")
                Dim dataLimite As Date = DateAdd("d", 30, x)

                If dataLimite <= datadevolver Then
                    frmMensagem.lblTexto.Text = "A renovação para esse empréstimo já chegou ao limite"
                    frmMensagem.Size = New Size(380, 250)
                    frmMensagem.btnOk.Location = New Point(150, 130)
                    frmMensagem.ShowDialog()
                Else
                    con.Open()
                    Dim cmd As New SqlCommand("UPDATE emprestimo Set dataDevolucao = DATEADD(dd, +15, dataDevolucao) WHERE codigo = @codigoEmprestimo", con)
                    cmd.Parameters.Add("@codigoEmprestimo", SqlDbType.Int).Value = CInt(frmRenovarEmprestimo.txtCodigoEmprestimo.Text)
                    cmd.ExecuteNonQuery()
                    ''fmr de msg aqui
                    frmMensagem.lblTexto.Text = "Este emprestimo foi renovado com sucesso." ''add a  nova data de devolução
                    frmMensagem.Size = New Size(380, 250)
                    frmMensagem.btnOk.Location = New Point(150, 130)
                    frmMensagem.ShowDialog()
                    con.Close()
                    AtualizarGrid()
                End If

            ElseIf bloqueio = True Then
                ''frm de msg aqui
                frmMensagem.lblTexto.Text = "Não é possivel fazer a " + vbCrLf + "renovação deste emprestimo " + vbCrLf + "pois o leitor esta bloqueado"
                frmMensagem.Size = New Size(350, 200)
                frmMensagem.btnOk.Location = New Point(140, 120)
                frmMensagem.ShowDialog()
            End If

        End If

    End Sub

    Private Sub btnDevolucao_Click(sender As Object, e As EventArgs) Handles btnDevolucao.Click
        Try
            If dtgEmprestimo.SelectedRows.Count <> 0 Then
                frmDevolucao.txtCodigoEmprestimo.Text = dtb.Rows(dtgEmprestimo.CurrentRow.Index).Item("Codigo Emprestismo")
            Else
                frmDevolucao.txtCodigoEmprestimo.Text = ""
            End If
        Catch a As Exception
        End Try
        frmDevolucao.ShowDialog()

        If frmDevolucao.DialogResult = Windows.Forms.DialogResult.OK Then
            con.Open()

            Dim adp As New SqlDataAdapter("SELECT l.nome AS [Nome do Leitor], l.codigo AS [codigo leitor], l.email AS [email] , li.codigo AS [Codigo do Livro] , li.nome AS [Nome do Livro], e.entregue AS [entregue], e.dataDevolucao AS [Prazo da Devolução], e.dataEmprestimo AS [Data do Emprestimo] FROM emprestimo e INNER JOIN Leitor l ON L.codigo = e.codLeitor INNER JOIN LIVRO li ON li.codigo = e.codLivro WHERE e.codigo = @codigo", con)
            adp.SelectCommand.Parameters.Add("@codigo", SqlDbType.Int).Value = CInt(frmDevolucao.txtCodigoEmprestimo.Text)
            dtbEmprestimo.Clear()
            adp.Fill(dtbEmprestimo)

            email = dtbEmprestimo.Rows(0).Item("email")

            If dtbEmprestimo.Rows(0).Item("entregue") = False Then

                frmFinalizarDevolucao.txtNomeLeitor.Text = dtbEmprestimo.Rows(0).Item("Nome do Leitor")
                frmFinalizarDevolucao.txtNomeLivro.Text = dtbEmprestimo.Rows(0).Item("Nome do Livro")
                frmFinalizarDevolucao.txtDataEmprestimo.Text = dtbEmprestimo.Rows(0).Item("Data do Emprestimo")
                frmFinalizarDevolucao.txtDataPrevista.Text = dtbEmprestimo.Rows(0).Item("Prazo da Devolução")

                frmFinalizarDevolucao.ShowDialog()

                If frmFinalizarDevolucao.finalizar = True Then

                    Dim prazo As New Date
                    prazo = dtbEmprestimo.Rows(0).Item("Prazo da Devolução")

                    If prazo < Today Then
                        ''frm de msg aqui
                        frmMensagem.lblTexto.Text = "O emprestimo que finalizou " + vbCrLf + "passou do prazo, saiba que " + vbCrLf + "o leitor já está bloqueado."
                        frmMensagem.Size = New Size(300, 200)
                        frmMensagem.btnOk.Location = New Point(120, 130)
                        frmMensagem.ShowDialog()
                        'motivoBanco = "Atraso na devolução"
                        'motivoEmail = "Atraso na devolução, por favor contate com a coordenação da bilioteca para que haja um acordo quanto a liberação da sua utilização"
                        '''frm de msg de confirmação aqui
                        'If MsgBox("Deseja realmente bloquear?", MsgBoxStyle.YesNo, "Bloquear") = MsgBoxResult.Yes Then
                        '    Dim bloqueia As New SqlCommand("UPDATE Leitor SET Bloqueio = 'True', motivoBloqueio = @motivo WHERE codigo = @codigoLeitor", con)
                        '    bloqueia.Parameters.Add("@codigoLeitor", SqlDbType.Int).Value = CInt(dtbEmprestimo.Rows(0).Item("codigo leitor"))
                        '    bloqueia.Parameters.Add("@motivo", SqlDbType.VarChar, 400).Value = motivoBanco
                        '    bloqueia.ExecuteNonQuery()
                        '    atrasoEmail = New Thread(AddressOf mandaEmail)
                        '    atrasoEmail.Start()
                        'End If
                    End If

                    Dim atualizar As New SqlCommand("UPDATE Livro SET Quant = quant + 1 WHERE Codigo = @codigoLivro", con)
                    atualizar.Parameters.Add("@codigoLivro", SqlDbType.Int).Value = CInt(dtbEmprestimo.Rows(0).Item("Codigo do Livro"))
                    atualizar.ExecuteNonQuery()
                    ''frm de msg devolvido
                    'frmMensagem.lblTexto.Text = "Emprestimo finalizado com sucesso! O exemplar devolvido voltou a constar com disponivel"
                    'frmMensagem.Size = New Size(480, 250)
                    'frmMensagem.btnOk.Location = New Point(250, 130)
                    'frmMensagem.ShowDialog()

                End If

                If frmMensagem.DialogResult = Windows.Forms.DialogResult.OK Then

                    Dim devolver As New SqlCommand("INSERT INTO devolucao VALUES(@codEmprestimo, getDate() )", con)
                    devolver.Parameters.Add("@codEmprestimo", SqlDbType.Int).Value = CInt(frmDevolucao.txtCodigoEmprestimo.Text)
                    devolver.ExecuteNonQuery()
                    motivoBanco = "Perda do livro"
                    ''bloqueando o leitor independente se tem o livro novo ou nao
                    If frmFinalizarDevolucao.perda = True Then
                        motivoEmail = "Prezada(o) " + dtbEmprestimo.Rows(0).Item("Nome do Leitor") + ", gostariamos de lembra-la(o) que ao fazer uso dos livro da bilioteca voce se torna responsavel pelo mesmo e assim caso ocorra a perda ou o extravio, o(a) senhor(a) tem o dever e a obrigação de recompor o acervo da biblioteca com um exemplar, de assunto e valor equivalente ao extraviado. E que ate que o livro seja reposto o(a) senhor(a) permanecerá bloqueado. Agradecemos a Atenção e contamos com você para continuar a oferecer livros em bom estado e de boa qualidade para nossa sociedade"
                        Dim bloqueia As New SqlCommand("UPDATE Leitor SET Bloqueio = 'True', motivoBloqueio = @motivo WHERE codigo = @codigoLeitor", con)
                        bloqueia.Parameters.Add("@codigoLeitor", SqlDbType.Int).Value = CInt(dtbEmprestimo.Rows(0).Item("codigo leitor"))
                        bloqueia.Parameters.Add("@motivo", SqlDbType.VarChar, 400).Value = motivoBanco
                        bloqueia.ExecuteNonQuery()
                        mde = True
                        devolucaoEmail = New Thread(AddressOf mandaEmail)
                        devolucaoEmail.Start()
                    End If
                End If

            ElseIf dtbEmprestimo.Rows(0).Item("entregue") = True Then
                ''frm de msg aqui
                frmMensagem.lblTexto.Text = "Este Emprestimo já foi finalizado."
                frmMensagem.Size = New Size(300, 200)
                frmMensagem.btnOk.Location = New Point(150, 130)
                frmMensagem.ShowDialog()
            End If

            con.Close()
            AtualizarGrid()
        End If
    End Sub

    Private Sub lblSair_Click(sender As Object, e As EventArgs) Handles lblSair.Click

    End Sub

    Private Sub Label2_Click(sender As Object, e As EventArgs) Handles Label2.Click
        Me.Close()
        frmMenu.Show()
    End Sub

End Class