--TRIGGER REAL OFICIAL

---ATUALIZAR DATA DA DEVOLUÇÃO
CREATE TRIGGER fi_EMPRESTIMO
ON emprestimo
FOR INSERT
AS BEGIN
SET NOCOUNT ON;
	UPDATE emprestimo SET dataDevolucao = DATEADD(dd, +15, (select dataEmprestimo from inserted)) WHERE codigo = (select codigo from inserted )
END

----VERIFICA A QUANTIDADE DE LIVRO E ATUALIZA A QUANTIDADE E A DISPONIBILIDADE DO LIVRO APOS A EFETUAÇÃO DO EMPRESTIMO 
CREATE TRIGGER ai_Emprestimo
ON emprestimo
AFTER INSERT
AS BEGIN 
 SET NOCOUNT ON;
	if (select li.quant from livro LI INNER JOIN inserted I ON I.codLivro = LI.CODIGO WHERE li.codigo  = (select codLivro FROM INSERTED) ) = 1 BEGIN
		update livro set Disponibilidade = 'false' WHERE Codigo = (SELECT codlivro FROM inserted)
	end
	else begin
		update livro set Disponibilidade = 'true' WHERE Codigo = (SELECT codlivro FROM inserted)
	END 	

	UPDATE livro SET Quant = ((select li.quant from livro LI INNER JOIN inserted I ON I.codLivro = LI.CODIGO) - 1 ) WHERE Codigo = (SELECT codlivro FROM inserted)
END

------DEVOLUÇÃO 
CREATE TRIGGER af_DEVOLUCAO
ON devolucao
AFTER INSERT 
AS BEGIN 
SET NOCOUNT ON;
	UPDATE Emprestimo SET Entregue = 'true' WHERE Codigo = (SELECT codEmprestimo FROM inserted)

	if ((select quant from livro  WHERE Codigo = (SELECT codLivro FROM Emprestimo WHERE Codigo = (SELECT codEmprestimo FROM inserted))) > 0 ) begin
		UPDATE livro SET Disponibilidade = 'true' WHERE Codigo = (SELECT codLivro FROM Emprestimo WHERE Codigo = (SELECT codEmprestimo FROM inserted))
	end
END
-----DELETE ESTANTES E PRATELEIRAS
CREATE TRIGGER AD_PRATELEIRAS
ON estante
INSTEAD OF DELETE 
AS BEGIN 
SET NOCOUNT ON;
	DECLARE CursorPrateleiras
		CURSOR FOR
			SELECT Codigo FROM  deleted
		DECLARE 
				@codEstante INTEGER

		OPEN CursorPrateleiras
		--Atribuindo valores do select nas variáveis
		FETCH NEXT FROM CursorPrateleiras INTO @codEstante
		WHILE @@FETCH_STATUS = 0
		BEGIN

			delete from Localizacao  where codEstante = (SELECT codEstante from deleted)
			--DELETE FROM Prateleira WHERE codEstante = (SELECT CODIGO FROM deleted) VAI TER TRIGGER PROPRIA

		FETCH NEXT FROM CursorPrateleiras INTO @codEstante
		END

	CLOSE CursorPrateleiras
	DEALLOCATE CursorPrateleiras
END

CREATE TRIGGER AD_LocalDosLIVROS----------------------------------------------------------------------------------------------------------------------------------------
ON PRATELEIRA
FOR DELETE 
AS BEGIN 
SET NOCOUNT ON;
	DECLARE CursorLocalLivros
		CURSOR FOR
			SELECT Codigo FROM  deleted
		DECLARE 
				@codPrateleira INTEGER

		OPEN CursorLocalLivros
		--Atribuindo valores do select nas variáveis
		FETCH NEXT FROM CursorLocalLivros INTO @codPrateleira
		WHILE @@FETCH_STATUS = 0
		BEGIN
			DELETE FROM Localizacao WHERE codPrateleira  = (SELECT Codigo FROM deleted )
		FETCH NEXT FROM CursorLocalLivros INTO @codPrateleira
		END

	CLOSE CursorLocalLivros
	DEALLOCATE CursorLocalLivros
END



----DISPONIBILIDADE ESTANTE

CREATE TRIGGER AF_PrateleiraNaEstante
ON PRATELEIRA
AFTER INSERT 
AS BEGIN 
SET NOCOUNT ON;
	IF (SELECT ALOCADASP FROM ESTANTE WHERE CODIGO = (SELECT codEstante FROM inserted)) < (SELECT CAPACIDADE FROM Estante WHERE CODIGO= (SELECT codEstante FROM inserted))  BEGIN
		UPDATE ESTANTE SET AlocadasP  = ( SELECT alocadasP FROM ESTANTE WHERE CODIGO = (SELECT codEstante FROM inserted) ) + 1 WHERE codigo = (SELECT codEstante  FROM inserted ) 		
		update Prateleira set Posicao = (SELECT alocadasP FROM ESTANTE WHERE CODIGO = (SELECT codEstante FROM inserted)) WHERE codigo = (SELECT Codigo FROM inserted)
	END
	ELSE BEGIN
		update Estante  set Disponibilidade = 'false' WHERE codigo = (SELECT codEstante FROM inserted )
	END 
END

---DELETE PRATELEIRA
CREATE  TRIGGER AD_ESPAÇOESTANTE
ON PRATELEIRA
AFTER DELETE
AS BEGIN
SET NOCOUNT ON;
	UPDATE ESTANTE SET Capacidade = ( SELECT CAPACIDADE FROM ESTANTE WHERE CODIGO = (SELECT codEstante FROM deleted)) + 1 WHERE codigo = (SELECT codEstante  FROM deleted  )
	DELETE FROM Localizacao WHERE codPrateleira = (SELECT CODIGO FROM DELETED )
END

--ALTERAR PRATELEIRA
CREATE TRIGGER au_LocalLivrosComPrateleiraAlterada
ON PRATELEIRA
AFTER UPDATE
AS BEGIN
SET NOCOUNT ON;
	IF (SELECT GENERO FROM inserted ) <> (SELECT GENERO FROM deleted) BEGIN
		DELETE FROM Localizacao WHERE codPrateleira = (SELECT CODIGO FROM DELETED )
	end
END


select * from leitor
select * from Livro 
 select * from estante
select * from Prateleira

DELETE FROM Estante WHERE codigo = 18