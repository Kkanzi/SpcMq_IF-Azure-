using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DsiMq_IF.UTIL
{
    public class DBQuery
    {
        public static readonly string SQLiteInfoCreateTable = "CREATE TABLE scheinfo (IDX INTEGER primary key, GUBUN varchar(1), NAME varchar(50), JOB varchar(50), SEC INTEGER, USEYN varchar(1), EXTFILE varchar(200));";
        public static readonly string SQLiteInfoInsertRecord = "insert into scheinfo (GUBUN, NAME, JOB, SEC, USEYN, EXTFILE) values ('', '측정데이터 전송', 'syncInspectData', 600, 'Y', ''); " +
                                                        "insert into scheinfo (GUBUN, NAME, JOB, SEC, USEYN, EXTFILE) values ('', '부서코드 전송', 'syncDeptInfo', 600, 'Y', ''); " +
                                                        "insert into scheinfo (GUBUN, NAME, JOB, SEC, USEYN, EXTFILE) values ('', '공장코드 전송', 'syncFactInfo', 600, 'Y', ''); " +
                                                        "insert into scheinfo (GUBUN, NAME, JOB, SEC, USEYN, EXTFILE) values ('', '공정정보 전송', 'syncProcInfo', 600, 'Y', ''); " +
                                                        "insert into scheinfo (GUBUN, NAME, JOB, SEC, USEYN, EXTFILE) values ('', '라인정보 전송', 'syncLineInfo', 600, 'Y', ''); " +
                                                        "insert into scheinfo (GUBUN, NAME, JOB, SEC, USEYN, EXTFILE) values ('', '품번정보 전송', 'syncGoodsInfo', 600, 'Y', ''); " +
                                                        "insert into scheinfo (GUBUN, NAME, JOB, SEC, USEYN, EXTFILE) values ('', '검사항목 전송', 'syncInspectInfo', 600, 'Y', ''); " +
                                                        "insert into scheinfo (GUBUN, NAME, JOB, SEC, USEYN, EXTFILE) values ('', '스펙정보 전송', 'syncGoodsInspect', 600, 'Y', ''); " +
                                                        "insert into scheinfo (GUBUN, NAME, JOB, SEC, USEYN, EXTFILE) values ('', 'OCAP 전송', 'syncOCAPInfo', 600, 'Y', ''); " +
                                                        "insert into scheinfo (GUBUN, NAME, JOB, SEC, USEYN, EXTFILE) values ('', '불량조치유형정보 전송', 'syncSpecOutActionInfo', 600, 'Y', ''); " +
                                                        "insert into scheinfo (GUBUN, NAME, JOB, SEC, USEYN, EXTFILE) values ('', '최종현품처리정보 전송', 'syncSpecOutFinalPointInfo', 600, 'Y', ''); " +
                                                        "insert into scheinfo (GUBUN, NAME, JOB, SEC, USEYN, EXTFILE) values ('', '불량명정보 전송', 'syncSpecOutNameInfo', 600, 'Y', ''); " +
                                                        "insert into scheinfo (GUBUN, NAME, JOB, SEC, USEYN, EXTFILE) values ('', '이상점 발생 전송', 'syncAbnormalList', 600, 'Y', ''); " +
                                                        "insert into scheinfo (GUBUN, NAME, JOB, SEC, USEYN, EXTFILE) values ('', '스펙아웃정보 전송', 'syncSpecOutList', 600, 'Y', ''); " +
                                                        "insert into scheinfo (GUBUN, NAME, JOB, SEC, USEYN, EXTFILE) values ('', '통계치 데이터 전송', 'syncStatData', 600, 'Y', ''); "
                                                        ;

        public static readonly string SQLiteInfoSelectQuery = "SELECT IDX, GUBUN, NAME, JOB, SEC, USEYN, EXTFILE FROM scheinfo";

        public static readonly string AbnormalListProc = "USP_GETSET_MQ_ABNORMAL_RESULT";

        public static readonly string DeptInfoProc = "USP_GETSET_MQ_DEPARTMENT_LIST";

        public static readonly string FactInfoProc = "USP_GETSET_MQ_FACTORY_LIST";

        public static readonly string GoodsInfoProc = "USP_GETSET_MQ_GOODS_LIST";

        public static readonly string GoodsInspectProc = "USP_GETSET_MQ_GOODSINSPECT_LIST";

        public static readonly string InspectDataProc = "USP_GETSET_MQ_INSPECT_DATA";

        public static readonly string InspectInfoProc = "USP_GETSET_MQ_INSPECT_LIST";

        public static readonly string LineInfoProc = "USP_GETSET_MQ_LINE_LIST";

        public static readonly string OCAPInfoProc = "USP_GETSET_MQ_OCAPMASTER_LIST";

        public static readonly string ProccessInfoProc = "USP_GETSET_MQ_PROCESS_LIST";

        public static readonly string SpecOutActionInfoProc = "USP_GETSET_MQ_SPECOUTACTION_LIST";

        public static readonly string SpecOutFinalInfoProc = "USP_GETSET_MQ_SPECOUTFINALPOINT_LIST";

        public static readonly string SpecOutListProc = "USP_GETSET_MQ_SPECOUT_RESULT";

        public static readonly string SpecOutNameInfoProc = "USP_GETSET_MQ_SPECOUTNAME_LIST";

        public static readonly string StaticsDataProc = "USP_GETSET_MQ_CALC_LIST_TEST";



    }
}
