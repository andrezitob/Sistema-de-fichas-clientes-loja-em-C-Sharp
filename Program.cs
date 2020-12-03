using Newtonsoft.Json;//instalado com NuGet
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

//client record system
//commit test
namespace sistema_Fichas_de_loja
{
    class Program
    {
        class Mercadoria
        {
            public string strNome;
            public float fValor;
            public string datComprado;
            public Mercadoria(string nome, float valor, string data)
            {
                strNome = nome;
                fValor = valor;
                datComprado = data;
            }
        }
        class Pagamento
        {
            public float fValor;
            public string datPago;
            public Pagamento(float valor, string data)
            {
                fValor = valor;
                datPago = data;
            }
        }
        class Ficha
        {
            public List<Mercadoria> mercadorias = new List<Mercadoria>();
            public List<Pagamento> pagamentos = new List<Pagamento>();
            public string dateOfCreation;
            public string strName;
            public float fTotal;
            public string datUltimoPagamento;
            public string datUltimaCompra;
            public string dateOfLastUpdate;
            public string numTelefone;
            public Ficha(string nome)
            {
                dateOfCreation = CurrentDate();
                strName = nome;
                fTotal = 0;
            }
        }
        //notas: organizar a logica dos if simples e unificar, tipo se mercadorias.Count == 0 write não existem mercadorias else executar codigo normalmente, ao ives de se mercadorias.Count != 0 executar o codigo normal else write blabla
        //indicador Lobby, insideficha, edit merc, edit ficha
        //OPÇÃO AUTOCLEAN, a cada input o console limpa, assim apenas fica visível no momento onde o usuário se encontra, (ficha opções, lobby etc), os comenados do devido "local"
        //usar um design pattern diferente?
        //end of classes --------------------------------------------

        //-v- : itens um espaçamento a mais fazem parte deste item
        //-t- : itens um espaçamento a mais fazem parte deste item mas só são chamados se uma condição é verdadeira
        //ex:
        //-v-a                                 1.0
        //----eufassopartedea                   1.1
        //----eutbmfassopartedea                1.2
        //-v-b                                 2.0
        //----eufassoparteapenasdeb             2.1
        //-t-c                                 3.0   (executa se uma condição for verdadeira)



        //fluxo do programa:
        //
        //Serialization                                      0
        //v-lobby                                            1.0.0.0.0.0
        //-v-CreateFicha                                     1.1.0.0.0.0
        //--t-DoesItExists                                   1.1.0.0.0.0 / 1.2.1.6.1.0 //como doesitexists sempre e unicamente é chamado por createficha e changefichaname , considero como "parte de cada um"
        //-----InsideFicha                                   1.1.1.0.0.0

        //-t-SearchFicha                                     1.2.0.0.0.0
        //--v-InsideFicha                                    1.2.1.0.0.0
        //-----AddMercadoria                                 1.2.1.1.0.0
        //-----AddPagamento                                  1.2.1.2.0.0
        //-----ShowAllMercadorias                            1.2.1.3.0.0
        //-----ShowAllPagamentos                             1.2.1.4.0.0
        //---t-SearchMercadoria                              1.2.1.5.0.0
        //----v-EditMercadoria                               1.2.1.5.1.0
        //-------ChangeMercadoriaValue                       1.2.1.5.1.1 
        //-------ChangeMercadoriaName                        1.2.1.5.1.2
        //-------ChangeMercadoriaData                        1.2.1.5.1.3
        //-------DeleteMercadoria                            1.2.1.5.1.4
        //---v-EditFicha                                     1.2.1.6.0.0
        //----v-ChangeFichaName                              1.2.1.6.1.0
        //-----t-DoesItExists                                1.2.1.6.1.0 / 1.1.0.0.0.0
        //------ChangeFichaPhone                             1.2.1.6.2.0
        //------DeleteMercadoriasandPagamentosHistoric       1.2.1.6.3.0
        //----t-DeleteFicha                                  1.2.1.6.4.0

        //---ShowAllFichasAlphabetically                     1.3.0.0.0.0

        //---ShowAllFichasDecreasingly                       1.4.0.0.0.0
        //Deserialization                                    0

        //public functional void
        public static string CurrentDate()
        {
            DateTime today = DateTime.Today;
            return today.ToString("dd/MM/yyyy");
        }
        static void Main(string[] args)
        {
            string strUserInput;
            int bedtime = 550;
            bool bMustWriteComms = false;//must write commands
            bool bAutoClean = true;//não implementado ainda
            int intMaxNumOfCharByPersonsName = 70;//número máximo de caracteres para nome de uma pessoa || Max number of characters by person name
            int intMaxNumOfCharByMerchName = 45;//número máximo de caracteres para nome de mercadoria

            List<Ficha> fichas = new List<Ficha>();
            DeserializationHandler();

            if (fichas.Count == 0)
            {
                Console.WriteLine("Nenhuma ficha existente em 'database.json'");
            }
            else if (fichas.Count == 1)
            {
                Console.WriteLine("Carregada " + fichas.Count + " ficha");
            }
            else
            {
                Console.WriteLine("Carregadas " + fichas.Count + " fichas");
            }
            Console.Write("Continuar.");
            Console.ReadLine();
            //Lobby : Aplicação principal
            Lobby();//uma coisa que vai ser muito vista são void(funções) que pedem indexficha e indexmercadoria, isso pq não são nested voids, por isso temos que ficar passando esses valores a toda hora, note que int indexficha de uma função qualquer é diferente de int indexficha de outra, isso pq NÃO SÃO NESTED VOIDS, mas os valores passados devem ser iguais, pois se referem a uma mesma ficha ou mercadoria.
            SerializationHandler();
            //end of aplication------------------------------------------------------

            void DeserializationHandler()
            {
                try
                {
                    StreamReader srtest = new StreamReader("database.json");//abre um processo que "observa" o arquivo json
                    srtest.Close();
                }
                catch
                {
                    Console.WriteLine("Não foi possivel encontrar o arquivo 'database.json' na pasta fonte.");
                    Console.WriteLine("Criar um novo arquivo limpo que guarda todas as informações de fichas? (S para sim)");
                    //string resposta = Console.ReadLine();
                    if (Console.ReadLine() == "S")
                    {
                        File.Create("database.json").Dispose();
                    }
                    else
                    {
                        return;
                        //end of aplication
                    }
                }
                //0 Deserialization
                StreamReader sr = new StreamReader("database.json");//abre um processo que "observa" o arquivo json
                string strFichasDeserialized = sr.ReadToEnd();//transforma todo seu conteudo numa string
                sr.Close();//fecha o processo de "observar" o arquivo json, assim ele pode ser "observado" por um processo diferente um pouco antes do final da aplicação (serialização)

                if (strFichasDeserialized != string.Empty && strFichasDeserialized != "null")
                {
                    try
                    {
                        fichas = JsonConvert.DeserializeObject<List<Ficha>>(strFichasDeserialized);//cria uma lista com os dados do arquivo
                    }
                    catch
                    {
                        Console.WriteLine("Erro ao converter as informações do arquivo 'database.json' para um objeto.");
                        Console.WriteLine("Criar um novo arquivo limpo que guarda todas as informações de fichas? (S para sim)");
                        if (Console.ReadLine() == "S")
                        {
                            File.Create("database.json").Dispose();
                            DeserializationHandler();
                        }
                        else
                        {
                            return;//end of app
                        }

                        Console.ReadLine();
                    }
                }
            }
            //0 Serialization
            void SerializationHandler()
            {
                var strFichasSerialized = JsonConvert.SerializeObject(fichas);
                File.WriteAllText("database.json", strFichasSerialized);
                Console.WriteLine("Saindo...");
                Thread.Sleep(bedtime);
            }




            //show commands - voids ---------------------------------------------------------
            void LobbyCommands()//1.0.0.0
            {
                Console.WriteLine("#################################################################################################################################");
                Console.WriteLine("# 1 criar ficha");//
                Console.WriteLine("# 2 pesquisar ficha");//
                Console.WriteLine("# 3 listar todas as fichas em ordem alfabética");//
                Console.WriteLine("# 4 listar todas as fichas em ordem decrescente (maior valor de divida a menor)");//
                Console.WriteLine("# l limpar console");//
                Console.WriteLine("# 0 sair e SALVAR");//
                Console.WriteLine("#################################################################################################################################");
            }
            void FichaInsideCommands()
            {
                Console.WriteLine("#################################################################################################################################");
                Console.WriteLine("# 1 adcionar mercadoria comprada (dívida+)");//
                Console.WriteLine("# 2 adcionar pagamento feito (dívida-)");//
                Console.WriteLine("# 3 mostrar o histórico de mercadorias compradas por essa pessoa");//
                Console.WriteLine("# 4 mostrar o histórico de pagamentos feitos por pessoa");//
                Console.WriteLine("# 5 editar mercadoria específica");//
                Console.WriteLine("# 6 editar ficha");//
                Console.WriteLine("# l limpar console");//
                Console.WriteLine("# 0 voltar");//
                Console.WriteLine("#################################################################################################################################");
            }
            void EditFichaCommands()
            {
                Console.WriteLine("#################################################################################################################################");
                Console.WriteLine("# 0 cancelar e voltar");//
                Console.WriteLine("# 1 editar nome");//
                Console.WriteLine("# 2 adcionar/editar número de telefone");//  //
                Console.WriteLine("# 3 limpar histórico (a divida total continua a mesma, mas todas as informações relacionadas a compras e pagamentos são deletados)");
                Console.WriteLine("# 4 excluir ficha");//
                Console.WriteLine("#################################################################################################################################");
            }
            void EditMercadoriaCommands()
            {
                Console.WriteLine("#################################################################################################################################");
                Console.WriteLine("# 0 voltar");//
                Console.WriteLine("# 1 mudar valor mercadoria");//
                Console.WriteLine("# 2 mudar nome mercadoria");//
                Console.WriteLine("# 3 mudar data de compra");
                Console.WriteLine("# 4 excluir mercadoria");//
                Console.WriteLine("# l limpar console");
                Console.WriteLine("#################################################################################################################################");
            }


            //functional voids-----------------------------(são chamados por mais de uma parte diferente do programa)    
            bool DoesItExists(string strPersonsName)
            {
                bool exists = false;
                for (int i = 0; i < fichas.Count; i++)
                {
                    if (strPersonsName == fichas[i].strName)
                    {
                        exists = true;
                    }
                }
                return exists;
            }
            void lilbitch()
            {
                Console.Write("__");
            }
            void lilbitchlob()
            {
                Console.Write("lob__");
            }
            void lilbitchfop()
            {
                Console.Write("fop__");
            }
            void lilbitchedm()
            {
                Console.Write("edm__");
            }
            void lilbitchedf()
            {
                Console.Write("edf__");
            }
            string FloatInputHandler()
            {
                strUserInput = Console.ReadLine();
                while (true)//handle wrong input
                {
                    bool erro = false;
                    try
                    {
                        Convert.ToSingle(strUserInput);
                    }
                    catch
                    {
                        Console.WriteLine("caracteres invalidos, tente novamente");
                        lilbitch();
                        strUserInput = Console.ReadLine();
                        erro = true;
                    }
                    if (!erro)
                    {
                        return strUserInput;
                    }
                }
            }
            string DataInputHandler()
            {
                strUserInput = Console.ReadLine();
                return "";
            }
            //end of functional voids

            //function - voids -----------------------------------------------------------------
            void Help()
            {
                Console.WriteLine("Esse programa tem o intuito de servir como banco de dados que segue um simples sistema");
                Console.WriteLine("de fichas, onde há o nome da pessoa na qual a ficha deriva-se, mercadorias ");
                Console.WriteLine("compradas a prazo (onde não há juros ou 'prazos' de pagamento) e pagamentos feitos.");
                Console.WriteLine("Aqui temos uma variedade de informações importantes que são armazenadas, como um histórico");
                Console.WriteLine("indivídual tanto de compras quanto de pagamentos, assim como as respectivas datas das de-");
                Console.WriteLine("vidas ações, data de criação de ficha, data de atualização de ficha");
            }
            void OpcoesCommands()
            {
                Console.WriteLine("# 1 desativar AutoClean");
            }
            //1
            void Lobby()//1.0.0.0.0.0
            {
                LobbyCommands();
                bool lobbyLoop = true;
                while (lobbyLoop)
                {
                    if (bMustWriteComms)
                    {
                        LobbyCommands();
                        bMustWriteComms = false;
                    }
                    lilbitchlob();
                    strUserInput = Console.ReadLine();
                    if (strUserInput == "0")
                    {
                        lobbyLoop = false;
                    }
                    else if (strUserInput == "1")
                    {
                        CreateFicha();
                    }
                    else if (strUserInput == "2")
                    {
                        SearchFicha();
                    }
                    else if (strUserInput == "3")
                    {
                        ShowAllFichasAlphabetically();
                    }
                    else if (strUserInput == "4")
                    {
                        ShowAllFichasDecreasingly();
                    }
                    else if (strUserInput == "l")
                    {
                        Console.Clear();
                        LobbyCommands();
                    }
                    else
                    {

                        //Console.WriteLine("Comando inválido, use c para ver todos os comandos disponíveis.");
                        //A implementar o print de comandos
                    }
                }
            }//end of Lobby

            //1.1.0.0.0.0-------------
            void CreateFicha()
            {
                Console.WriteLine("Diga-me o nome e sobrenome e/ou referencia(s) da pessoa (0 para cancelar e voltar)");
                lilbitchlob();
                string strPersonsName = Console.ReadLine();
                while (strPersonsName.Length > intMaxNumOfCharByPersonsName)
                {
                    Console.WriteLine("Num de caracteres excede " + intMaxNumOfCharByPersonsName + ", tente novamente");
                    lilbitchlob();
                    strPersonsName = Console.ReadLine();
                }

                if (strPersonsName != "0")
                {
                    bool nameAllreadyUsed = false;
                    if (fichas.Count != 0)
                    {
                        nameAllreadyUsed = DoesItExists(strPersonsName);
                    }
                    if (nameAllreadyUsed)
                    {
                        Console.WriteLine("O nome dado já faz parte de outra ficha!");
                        Thread.Sleep(bedtime / 2);
                        Console.WriteLine("Voltando...");
                        Thread.Sleep(bedtime / 2);
                    }
                    else
                    {
                        fichas.Add(new Ficha(strPersonsName));
                        fichas[fichas.Count - 1].dateOfCreation = CurrentDate();
                        fichas[fichas.Count - 1].dateOfLastUpdate = CurrentDate();
                        Console.WriteLine("ficha criada!");
                        FichaInside(fichas.Count - 1);//como a ficha acabou de ser adcionada, seu index está no final da lista
                        Thread.Sleep(bedtime);
                    }
                }
                else
                {
                    Console.WriteLine("Voltando...");
                }
            }//end of CreateFicha

            //1.2.0.0.0.0-------------
            void SearchFicha()
            {
                if (fichas.Count == 0)
                {
                    Console.WriteLine("Não existem fichas, use 1 para adcionar novas.");
                    Thread.Sleep(bedtime);
                }
                else
                {
                    Console.WriteLine("Diga-me o exato nome da pessoa na qual essa ficha pertence (0 cancelar)");
                    lilbitchlob();
                    string nome = Console.ReadLine();
                    if (nome == "0")
                    {
                        Console.WriteLine("Voltando...");
                        Thread.Sleep(bedtime);
                    }

                    else
                    {
                        bool encontrado = false;
                        for (int i = 0; i < fichas.Count; i++)
                        {
                            if (fichas[i].strName == nome)
                            {
                                encontrado = true;
                                FichaInside(i);
                            }
                        }
                        if (!encontrado)
                        {
                            Console.WriteLine("Pessoa não encontrada.");
                        }
                    }
                }
            }//end of SearchFicha

            //1.2.1.0.0
            void FichaInside(int indexFicha)
            {
                void fichaInformations()
                {
                    Console.WriteLine("#################################################################################################################################");
                    Console.Write("# " + fichas[indexFicha].strName);
                    Console.WriteLine(" deve:");
                    Console.WriteLine("#" + string.Format("{0:c}", fichas[indexFicha].fTotal));
                }

                fichaInformations();
                FichaInsideCommands();

                while (true)
                {
                    if (bMustWriteComms)
                    {
                        fichaInformations();
                        FichaInsideCommands();
                        bMustWriteComms = false;
                    }
                    lilbitchfop();
                    strUserInput = Console.ReadLine();
                    if (strUserInput == "0")
                    {
                        Console.WriteLine("Voltando para Lobby...");
                        bMustWriteComms = true;
                        Thread.Sleep(bedtime);
                        break;
                    }
                    else if (strUserInput == "1")
                    {
                        AddMercadoria(indexFicha);
                    }
                    else if (strUserInput == "2")
                    {
                        AddPagamento(indexFicha);
                    }
                    else if (strUserInput == "3")
                    {
                        ShowAllMercadorias(indexFicha);
                    }
                    else if (strUserInput == "4")
                    {
                        ShowAllPagamentos(indexFicha);
                    }
                    else if (strUserInput == "5")
                    {
                        SearchMercadoria(indexFicha);
                    }
                    else if (strUserInput == "6")
                    {
                        if (EditFicha(indexFicha) == true)//editficha é um void que funciona normalmente, mas que retorna true ou false caso a ficha seja excluida.
                        {
                            break;
                        }
                    }
                    else if (strUserInput == "l")
                    {
                        Console.Clear();
                        fichaInformations();
                        FichaInsideCommands();
                    }
                    else if (strUserInput == "c")
                    {
                        //comandos
                    }
                    else if (strUserInput == "a")
                    {
                        //ajuda
                    }
                    else
                    {
                        Console.WriteLine("Comando inválido, use c para ver todos os comandos disponíveis.");
                    }
                }
            }//end of FichaInside

            //1.2.1.1.0.0-------------------------------
            void AddMercadoria(int indexficha)
            {
                string nomeMerc;
                string valorMerc;
                Console.WriteLine("0 para parar e voltar");
                while (true)
                {
                    Console.WriteLine("Diga-me o valor da mercadoria (0,00)");
                    lilbitchfop();
                    valorMerc = FloatInputHandler();
                    float fValorMerc = Convert.ToSingle(valorMerc);
                    if (fValorMerc != 0)
                    {

                        Console.WriteLine("Diga-me o nome da mercadoria");
                        lilbitchfop();
                        nomeMerc = Console.ReadLine();
                        while (nomeMerc.Length > intMaxNumOfCharByMerchName && nomeMerc != "0")
                        {
                            Console.WriteLine("Num de caracteres excede " + intMaxNumOfCharByMerchName + ", tente novamente");
                            lilbitchfop();
                            nomeMerc = Console.ReadLine();
                        }
                        if (nomeMerc != "0")
                        {
                            fichas[indexficha].fTotal += fValorMerc;
                            fichas[indexficha].mercadorias.Add(new Mercadoria(nomeMerc, fValorMerc, CurrentDate()));
                            Console.WriteLine("Mercadoria adcionada! valor total de dívida: " + string.Format("{0:c}", fichas[indexficha].fTotal));
                            Thread.Sleep(bedtime - 100);
                        }
                        else
                        {
                            Console.WriteLine("Parando...");
                            Thread.Sleep(bedtime);
                            break;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Parando...");
                        Thread.Sleep(bedtime);
                        break;
                    }
                }
            }//end of AddMercadoria

            //1.2.0.2------------------------------
            void AddPagamento(int indexficha)
            {
                string valorPago;
                Console.WriteLine("Diga-me o valor pago (0,00)");
                lilbitchfop();
                valorPago = FloatInputHandler();
                float fValorPago = Convert.ToSingle(valorPago);
                if (fValorPago != 0)
                {
                    fichas[indexficha].dateOfLastUpdate = CurrentDate();
                    fichas[indexficha].pagamentos.Add(new Pagamento(fValorPago, CurrentDate()));

                    fichas[indexficha].fTotal -= fValorPago;
                    Console.WriteLine("Pagamento adcionado! valor total de dívida: " + string.Format("{0:c}", fichas[indexficha].fTotal));
                }
                Console.WriteLine("Voltando...");
                Thread.Sleep(bedtime);
            }//end of AddPagamentos

            //1.2.0.3
            void ShowAllMercadorias(int indexficha)
            {
                int NumMercs = fichas[indexficha].mercadorias.Count();
                if (NumMercs == 0)
                {
                    Console.WriteLine("Nenhuma mercadoria comprada por essa pessoa, use 1 para adcionar mercadorias.");
                }
                else
                {
                    for (int i = NumMercs - 1; i >= 0; i--)
                    {
                        Console.Write(string.Format("{0:c}", fichas[indexficha].mercadorias[i].fValor) + " ");
                        Console.Write(fichas[indexficha].mercadorias[i].datComprado + " ");
                        Console.WriteLine(fichas[indexficha].mercadorias[i].strNome + " ");
                    }
                }
            }//end of ShowAllMercadorias

            //1.2.0.4
            void ShowAllPagamentos(int indexficha)
            {
                int NumPags = fichas[indexficha].pagamentos.Count();
                if (NumPags == 0)
                {
                    Console.WriteLine("Nenhum pagamento foi efetuado, use 2 para adicionar pagamentos.");
                }
                else
                {
                    for (int i = NumPags - 1; i >= 0; i--)
                    {
                        Console.Write(string.Format("{0:c}", fichas[indexficha].pagamentos[i].fValor) + " ");
                        Console.WriteLine(fichas[indexficha].pagamentos[i].datPago);
                    }
                }

            }//end of ShowAllPagamentos

            //1.2.0.5----------------------------------
            void SearchMercadoria(int indexficha)
            {
                if (fichas[indexficha].mercadorias.Count != 0)
                {
                    Console.WriteLine("Diga-me o exato nome da mercadoria");
                    lilbitchfop();
                    string nome = Console.ReadLine();
                    bool encontrado = false;
                    if (nome == "0")
                    {
                        Console.WriteLine("Voltando...");
                        Thread.Sleep(bedtime);
                    }
                    else
                    {
                        List<int> SameName = new List<int>();
                        for (int i = fichas[indexficha].mercadorias.Count - 1; i >= 0; i--)
                        {
                            if (fichas[indexficha].mercadorias[i].strNome == nome)
                            {
                                encontrado = true;
                                SameName.Add(i);
                            }
                        }
                        if (!encontrado)
                        {
                            Console.WriteLine("Mercadoria não encontrada");
                        }
                        else if (SameName.Count > 1)
                        {
                            Console.WriteLine("Multiplas mercadorias de mesmo nome encontradas (" + nome + ")");
                            for (int i = 0; i < SameName.Count; i++)
                            {

                                Console.WriteLine("(" + (i + 1) + ") " + string.Format("{0:c}", fichas[indexficha].mercadorias[SameName[i]].fValor) + " " + fichas[indexficha].mercadorias[SameName[i]].datComprado);
                            }
                            Console.WriteLine("Use a numeração da mercadoria que desejas editar.");
                            lilbitchfop();
                            int ID = 0;
                            bool erro;
                            bool voltar = false;
                            while (true)
                            {
                                erro = false;
                                try
                                {
                                    ID = Convert.ToInt32(Console.ReadLine());
                                }
                                catch
                                {
                                    Console.WriteLine("Caracteres inválidos, tente novamente.");
                                    erro = true;
                                }
                                if (ID - 1 > SameName.Count)
                                {
                                    Console.WriteLine("Numero dado não condiz com as mercadorias listadas, tente novamente.");
                                    erro = true;
                                }
                                else if (ID == 0)
                                {
                                    voltar = true;
                                    break;
                                }
                                if (!erro)
                                {
                                    break;
                                }
                            }
                            if (voltar)
                            {
                                Console.WriteLine("Voltando...");
                                Thread.Sleep(bedtime);
                            }
                            else
                            {
                                EditMercadoria(indexficha, SameName[ID - 1]);
                            }
                        }
                        else
                        {
                            EditMercadoria(indexficha, SameName[0]);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Nenhuma mercadoria comprada por essa pessoa, use 1 para adcionar mercadorias.");
                }

            }//end of SearchMercadoria

            //1.2.0.5.1---------------------------------------------------
            void EditMercadoria(int indexficha, int indexmercadoria)//
            {
                Console.Write(fichas[indexficha].mercadorias[indexmercadoria].fValor + " ");
                Console.Write(fichas[indexficha].mercadorias[indexmercadoria].strNome + " ");
                Console.WriteLine(fichas[indexficha].mercadorias[indexmercadoria].datComprado);
                EditMercadoriaCommands();

                while (true)
                {
                    if (bMustWriteComms)
                    {
                        EditMercadoriaCommands();
                        bMustWriteComms = false;
                    }
                    lilbitchedm();
                    strUserInput = Console.ReadLine();
                    if (strUserInput == "0")
                    {
                        Console.WriteLine("Voltando para Ficha Opções...");
                        bMustWriteComms = true;
                        Thread.Sleep(bedtime);
                        break;
                    }
                    else if (strUserInput == "1")
                    {
                        ChangeMercadoriaValue(indexficha, indexmercadoria);
                    }
                    else if (strUserInput == "2")
                    {
                        ChangeMercadoriaName(indexficha, indexmercadoria);
                    }
                    else if (strUserInput == "3")
                    {
                        ChangeMercadoriaData(indexficha, indexmercadoria);
                    }
                    else if (strUserInput == "4")
                    {
                        DeleteMercadoria(indexficha, indexmercadoria);
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Comando inválido, use c para ver todos os comandos disponíveis.");
                    }
                }
            }//end of EditMercadoria

            void ChangeMercadoriaValue(int indexficha, int indexmercadoria)
            {
                Console.Write("mudar de");
                Console.Write(string.Format("{0:c}", fichas[indexficha].mercadorias[indexmercadoria].fValor));
                Console.WriteLine(" para: (0 voltar)");
                lilbitchedm();
                string valor = FloatInputHandler();
                float fvalor = Convert.ToSingle(valor);
                if (fvalor != 0)
                {
                    float oldvalor = fichas[indexficha].mercadorias[indexmercadoria].fValor;
                    fichas[indexficha].fTotal += fvalor - oldvalor;
                    fichas[indexficha].mercadorias[indexmercadoria].fValor = fvalor;
                    fichas[indexficha].dateOfLastUpdate = CurrentDate();
                    Console.WriteLine("Valor editado! valor total de dívida: " + string.Format("{0:c}", fichas[indexficha].fTotal));
                }
                else
                {
                    Console.WriteLine("Valor não editado.");
                }
            }
            void ChangeMercadoriaName(int indexficha, int indexmercadoria)
            {
                Console.Write("mudar '");
                Console.Write(fichas[indexficha].mercadorias[indexmercadoria].strNome);
                Console.WriteLine("' para: (0 voltar)");
                lilbitchedm();
                string nome = Console.ReadLine();
                while (nome.Length > intMaxNumOfCharByMerchName)
                {
                    Console.WriteLine("Num de caracteres excede " + intMaxNumOfCharByMerchName + ", tente novamente");
                    lilbitchedm();
                    nome = Console.ReadLine();
                }
                if (nome != "0")
                {
                    fichas[indexficha].mercadorias[indexmercadoria].strNome = nome;
                    fichas[indexficha].dateOfLastUpdate = CurrentDate();
                    Console.WriteLine("Nome editado!");
                }
                else
                {
                    Console.WriteLine("Nome não editado.");
                }
            }
            void ChangeMercadoriaData(int indexficha, int indexmercadoria)
            {
                Console.Write("mudar '");
                Console.Write(fichas[indexficha].mercadorias[indexmercadoria].datComprado);
                Console.WriteLine("' para:");
                lilbitchedm();
                string data = Console.ReadLine();
                if (data == "0")
                {
                    Console.WriteLine("Data não editada.");
                    Console.WriteLine("Voltando...");
                    Thread.Sleep(bedtime);
                }
                else
                {
                    //handle wrong input
                    fichas[indexficha].dateOfLastUpdate = CurrentDate();
                    fichas[indexficha].mercadorias[indexmercadoria].datComprado = data;
                    Console.WriteLine("Data editada!");
                }
            }
            void DeleteMercadoria(int indexficha, int indexmercadoria)
            {
                Console.WriteLine("excluir mercadoria? ('s' para sim)");
                lilbitchedm();
                strUserInput = Console.ReadLine();
                if (strUserInput == "s")
                {
                    float oldvalor = fichas[indexficha].mercadorias[indexmercadoria].fValor;
                    fichas[indexficha].fTotal -= oldvalor;
                    fichas[indexficha].mercadorias.RemoveAt(indexmercadoria);
                    Console.WriteLine("Mercadoria excluida, valor total de dívida: " + string.Format("{0:c}", fichas[indexficha].fTotal));
                    fichas[indexficha].dateOfLastUpdate = CurrentDate();
                    FichaInside(indexficha);
                }
                else
                {
                    Console.WriteLine("Mercadoria não excluida");
                }
            }
            //1.2.0.6---------------------------
            bool EditFicha(int indexficha)//bool pq retorna se a ficha foi ou não excluida.
            {
                EditFichaCommands();
                while (true)
                {
                    if (bMustWriteComms)
                    {
                        EditFichaCommands();
                        bMustWriteComms = false;
                    }
                    lilbitchedf();
                    strUserInput = Console.ReadLine();
                    if (strUserInput == "0")
                    {
                        Console.WriteLine("Voltando para Ficha Opções...");
                        bMustWriteComms = true;
                        Thread.Sleep(bedtime);
                        return false;//a partir do momento que retorna um valor, funções bool param na hora de serem executadas
                    }
                    else if (strUserInput == "1")
                    {
                        ChangeFichaName(indexficha);
                    }
                    else if (strUserInput == "2")
                    {
                        ChangeFichaPhone(indexficha);
                    }
                    else if (strUserInput == "3")
                    {
                        DeleteMercadoriasandPagamentosHistoric(indexficha);
                    }
                    else if (strUserInput == "4")
                    {
                        bool excluida = DeleteFicha(indexficha);//não precisava declarar um bool aqui, mas fiz isso por organização
                        if (excluida)
                        {
                            return true;//a partir do momento que retorna um valor, funções bool param na hora de serem executadas
                        }
                    }
                    else
                    {
                        Console.WriteLine("Comando inválido, use c para ver todos os comandos disponíveis.");
                    }
                }
            }//end of Edit Ficha
            void ChangeFichaName(int indexficha)
            {
                Console.WriteLine("Diga-me o novo nome dessa ficha (0 voltar):");
                lilbitchedf();
                string nome = Console.ReadLine();
                while (nome.Length > intMaxNumOfCharByPersonsName && nome != "0")
                {
                    Console.WriteLine("Num de caracteres excede " + intMaxNumOfCharByPersonsName + ", tente novamente");
                    lilbitchedf();
                    nome = Console.ReadLine();
                }
                if (nome != "0")
                {
                    bool jaexiste = DoesItExists(nome);
                    if (jaexiste)
                    {
                        Console.WriteLine("O nome dado já faz parte de outra ficha!");
                    }
                    else
                    {
                        fichas[indexficha].strName = nome;
                        Console.Write("Nome editado para: ");
                        Console.WriteLine(nome + "!");
                        fichas[indexficha].dateOfLastUpdate = CurrentDate();
                        Console.WriteLine("Voltando...");
                        Thread.Sleep(bedtime);
                    }
                }
                else
                {
                    Console.WriteLine("Nome não editado.");
                }
            }
            void ChangeFichaPhone(int indexficha)
            {
                Console.WriteLine("Diga-me o numero de telefone (ddd)0000-0000:(digite apenas os numeros)");
                lilbitchedf();
                string tel = Console.ReadLine();
                //handle wrong input
                while (tel.Length > 9 && tel != "0")
                {
                    Console.WriteLine("Num de caracteres maior que 9, tente novamente");
                    lilbitchedf();
                    tel = Console.ReadLine();
                    //handle wrong input
                }
                if (tel != "0")
                {
                    fichas[indexficha].numTelefone = tel;
                    fichas[indexficha].dateOfLastUpdate = CurrentDate();
                    Console.WriteLine("Numero de telefone editado/adcionado!");
                }
                else
                {
                    Console.WriteLine("Numero de telefone não editado.");
                }
            }

            void DeleteMercadoriasandPagamentosHistoric(int indexficha)
            {
                Console.WriteLine("excluir histórico? ('s' ou 'S' para sim)");
                lilbitchedf();
                strUserInput = Console.ReadLine();
                if (strUserInput == "s" || strUserInput == "S")
                {
                    fichas[indexficha].pagamentos.Clear();
                    fichas[indexficha].mercadorias.Clear();
                    Console.WriteLine("histórico excluido");
                    fichas[indexficha].dateOfLastUpdate = CurrentDate();
                }
                else
                {
                    Console.WriteLine("histórico não excluido");
                }
            }
            bool DeleteFicha(int indexficha)
            {
                bool excluida;
                Console.WriteLine("excluir ficha? ('s' ou 'S' para sim)");
                lilbitchedf();
                strUserInput = Console.ReadLine();
                if (strUserInput == "s" || strUserInput == "S")
                {
                    fichas.RemoveAt(indexficha);
                    Console.WriteLine("ficha excluida");
                    excluida = true;
                }
                else
                {
                    excluida = false;
                    Console.WriteLine("ficha não excluida");
                }
                return excluida;
            }
            //1.3-------------------------------
            void ShowAllFichasAlphabetically()
            {

                if (fichas == null)
                {
                    Console.WriteLine("Nenhuma ficha encontrada, use 1 para adicionar novas.");
                }
                else
                {
                    int NumFichas = fichas.Count();
                    List<string> sortednomes = new List<string>();
                    List<float> sortedtotals = new List<float>();
                    int numFichas = fichas.Count();
                    for (int i = 0; i < numFichas; i++)
                    {
                        sortednomes.Add(fichas[i].strName);
                    }
                    sortednomes.Sort();
                    for (int i = 0; i < numFichas; i++)
                    {
                        for (int j = 0; j < numFichas; j++)
                        {
                            if (sortednomes[i] == fichas[j].strName)
                            {
                                sortedtotals.Add(fichas[j].fTotal);
                                break;
                            }
                        }
                    }//0, 20 ,42
                    for (int i = 0; i < numFichas; i++)
                    {
                        Console.Write(sortednomes[i]);
                        Console.Write(" ");
                        Console.WriteLine(string.Format("{0:c}", sortedtotals[i]));
                    }
                }

            }//end of ShowAllFichasAlphabetically
            //1.4-----------------------------
            void ShowAllFichasDecreasingly()
            {

                if (fichas == null)
                {
                    Console.WriteLine("Nenhuma ficha encontrada, use 1 para adicionar novas.");
                }
                else
                {
                    int NumFichas = fichas.Count();
                    List<float> sortedtotals = new List<float>();
                    List<string> sortednomes = new List<string>();
                    for (int i = 0; i < fichas.Count(); i++)
                    {
                        sortedtotals.Add(fichas[i].fTotal);
                    }
                    sortedtotals.Sort();

                    for (int i = 0; i < fichas.Count; i++)
                    {
                        for (int j = 0; j < fichas.Count; j++)
                        {
                            if (sortedtotals[i] == fichas[j].fTotal)
                            {
                                sortednomes.Add(fichas[j].strName);
                                break;
                            }
                        }
                    }
                    for (int i = fichas.Count - 1; i >= 0; i--)
                    {
                        Console.Write(sortednomes[i]);
                        Console.Write(" ");
                        Console.WriteLine(string.Format("{0:c}", sortedtotals[i]));
                    }
                }
            }//end of ShowAllFichasDecreasingly
             //2.0---------------------------
             //lobby fichIn edFich edMerch

        }

    }
}
/*
                               rascunho: ajuda

1.0 Lobby- aq é possivel:

1.1 criar uma nova ficha, onde será necessrário especificar um nome
que deve ser diferente de nomes de fichas já existentes

1.2 pesquisar uma ficha, onde deve-se informar o exato nome pertencente a essa ficha,
feito isso se abrem Opções como adicionar pagamentos, compras, alterar o nome
em sí, numero de telefone, listar compras e pagamentos existentes, editar compras
e pagamentos já existentes e até mesmo excluir a ficha

1.2.0 Ficha Opções-
adcionar mercadoria, adciona uma compra. Deve-se ser informado o valor, 
nome do produto, e opcionalmente a data de compra, que é automáticamente dada como
o dia em que a mercadoria é adicionada, 

listar fichas em ordem alfabética, no qual todas as fichas existentes são
são organizadas em ordem alfabética por nome, e alguns dados são mostrados,
como logicamente o nome, valor de dívida total, numero de telefone, data de 
criação,

listar fichas em ordem decressente, similarmente a outra opção de listagem
de fichas mostra os mesmos dados, porém o critério de organização leva em conta
o valor de dívida, da pessoa que mais deve a que menos.

limpar console, limpa a tela e tudo escrito nela

*/
